﻿using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using SSW.Consulting.Application.Common.Interfaces;
using SSW.Consulting.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Persistence
{
    public class SampleDataSeeder
    {
        private readonly ISSWConsultingDbContext _context;

        private Dictionary<string, int> _skills;

        public SampleDataSeeder(ISSWConsultingDbContext context)
        {
            _context = context;
        }

        public async Task SeedAllAsync(byte[] profileData, CancellationToken cancellationToken)
        {
            var profiles = GetProfiles(profileData)
                .Where(p => !string.IsNullOrWhiteSpace(p.Profile))
                .ToList();

            await SeedSkillsAsync(profiles.SelectMany(p => p.Skills), cancellationToken);
            await SeedStaffMembers(profiles, cancellationToken);
            await SeedAchievementsAsync(cancellationToken);
        }

        private async Task SeedSkillsAsync(IEnumerable<string> newSkills, CancellationToken cancellationToken)
        {
            //nuke all skills (will be re-added later)
            _context.StaffMemberSkills.RemoveRange(await _context.StaffMemberSkills.ToArrayAsync(cancellationToken));

            _context.Skills.RemoveRange(await _context.Skills.ToArrayAsync(cancellationToken));
            await _context.SaveChangesAsync(cancellationToken);

            // group by lowered name to ensure no dupes
            var skillsToInsert = newSkills
                .Select(s => new
                {
                    Key = s.ToLower(),
                    Value = s
                })
                .GroupBy(p => p.Key)
                .Select(g => new Skill { Name = g.First().Value })
                .ToArray();
            await _context.Skills.AddRangeAsync(skillsToInsert, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _skills = await _context.Skills.ToDictionaryAsync(s => s.Name.ToLower(), s => s.Id);
        }

        private async Task SeedStaffMembers(IEnumerable<UserProfile> profiles, CancellationToken cancellationToken)
        {
            //remove removed profiles
            var profileNames = profiles.Select(p => p.Name).ToArray();
            var profilesToRemove = await _context
                .StaffMembers
                .Where(sm => !profileNames.Contains(sm.Name))
                .ToArrayAsync(cancellationToken);
            _context.StaffMembers.RemoveRange(profilesToRemove);
            await _context.SaveChangesAsync(cancellationToken);

            //add/update profiles
            var existingStaffMembers = await _context.StaffMembers.ToListAsync(cancellationToken);
            var existingAchievements = await _context.Achievements.ToListAsync(cancellationToken);
            profiles.ForEach(p =>
            {
                var staffMember = existingStaffMembers.FirstOrDefault(sm => sm.Name == p.Name) ?? new StaffMember();
                staffMember.Name = p.Name;
                staffMember.Title = p.Title;
                staffMember.StaffMemberSkills = p.Skills.Select(s => new StaffMemberSkill { SkillId = _skills[s.ToLower()] }).ToArray();
                staffMember.Profile = p.Profile;
                staffMember.TwitterUsername = p.TwitterUsername;

                if (staffMember.Id == 0)
                {
                    _context.StaffMembers.Add(staffMember);
                }

                SetupAchievement(existingAchievements, p.Name, p.Value);
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task SeedAchievementsAsync(CancellationToken cancellationToken)
        {
            var existingAchievements = await _context.Achievements.ToListAsync(cancellationToken);

            // tech quiz
            SetupAchievement(existingAchievements, "SSW Tech Quiz", 500);

            // prizes
            SetupAchievement(existingAchievements, "SSW Water Bottle", 50);
            SetupAchievement(existingAchievements, "Xiaomi Mi Band 4", 50);

            //talks
            SetupAchievement(existingAchievements, "Chinafy your apps + Lessons you can steal from China", 500);
            SetupAchievement(existingAchievements, "How to put a Penguin in a Cloud: Linux on Azure", 500);
            SetupAchievement(existingAchievements, "Clean Architecture with ASP.NET Core 3.0", 500);
            SetupAchievement(existingAchievements, "Real-time Face Recognition With Microsoft Cognitive Services", 500);

            // superpowers
            SetupAchievement(existingAchievements, "Angular Superpowers", 500);
            SetupAchievement(existingAchievements, "Azure Superpowers", 500);
            SetupAchievement(existingAchievements, ".NET Superpowers", 500);

            await _context.SaveChangesAsync(cancellationToken);
        }

        private void SetupAchievement(IEnumerable<Achievement> existingAchievements, string name, int value)
        {
            var achievement = existingAchievements
                .FirstOrDefault(a => a.Name.Equals(name, StringComparison.InvariantCulture))
                ?? new Achievement();

            achievement.Name = name;
            var codeData = Encoding.ASCII.GetBytes(name);
            achievement.Code = Convert.ToBase64String(codeData);
            achievement.Value = value;

            if (achievement.Id == 0)
            {
                _context.Achievements.Add(achievement);
            }
        }

        private IEnumerable<UserProfile> GetProfiles(byte[] profileData)
        {
            using (var stream = new MemoryStream(profileData))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                reader.Read();
                while (reader.Read())
                {
                    yield return new UserProfile
                    {
                        Name = reader.GetString(0)?.Trim(),
                        Title = reader.GetString(1)?.Trim(),
                        Skills = Enumerable.Range(2, 4)
                            .Select(i => reader.GetString(i)?.Trim())
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct()
                            .ToArray(),
                        Profile = reader.GetString(6)?.Trim(),
                        Value = (int)reader.GetDouble(7),
                        TwitterUsername = reader.GetString(8)?.Trim()
                    };
                }
            }
        }

        private class UserProfile
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public string[] Skills { get; set; }
            public string Profile { get; set; }
            public string TwitterUsername { get; set; }
            public int Value { get; set; }
        }
    }
}