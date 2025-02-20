using System.Collections.Generic;

namespace SSW.Consulting.Application.User.Queries.GetUserAchievements
{
    public class UserAchievementsViewModel
    {
        public int UserId { get; set; }
        public int Points { get; set; }

        public IEnumerable<UserAchievementViewModel> UserAchievements { get; set; }
    }
}
