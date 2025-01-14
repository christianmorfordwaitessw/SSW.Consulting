﻿using MediatR;
using SSW.Consulting.Application.Common.Interfaces;
using SSW.Consulting.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.System.Commands.SeedData
{
    public class SeedSampleDataCommand : IRequest
    {
    }

    public class SeedSampleDataCommandHandler : IRequestHandler<SeedSampleDataCommand>
    {
        private readonly ISSWConsultingDbContext _context;
        private readonly IProfileStorageProvider _storageProvider;

        public SeedSampleDataCommandHandler(
            ISSWConsultingDbContext context,
            IProfileStorageProvider storageProvider)
        {
            _context = context;
            _storageProvider = storageProvider;
        }

        public async Task<Unit> Handle(SeedSampleDataCommand request, CancellationToken cancellationToken)
        {
            var seeder = new SampleDataSeeder(_context);
            var profileData = await _storageProvider.GetProfileData();
            await seeder.SeedAllAsync(profileData, cancellationToken);

            return Unit.Value;
        }
    }
}
