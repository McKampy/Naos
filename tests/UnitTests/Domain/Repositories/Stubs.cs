﻿namespace Naos.Core.UnitTests.Domain.Repositories
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using EnsureThat;
    using Naos.Foundation.Domain;

#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1402 // File may only contain a single type
    public class StubEntity : TenantAggregateRoot<string>
    {
        public string Country { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }
    }

    public class StubDb
    {
        public string ExtTenantId { get; set; }

        public string Country { get; set; }

        public string Identifier { get; set; }

        public object ETag { get; internal set; }

        public string FullName { get; set; }

        public int YearOfBirth { get; set; }
    }

    public class StubHasNameSpecification : Specification<StubEntity> // TODO: this should be mocked
    {
        public StubHasNameSpecification(string firstName, string lastName)
        {
            EnsureArg.IsNotNull(firstName);
            EnsureArg.IsNotNull(lastName);

            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public string FirstName { get; }

        public string LastName { get; }

        public override Expression<Func<StubEntity, bool>> ToExpression()
        {
            return p => p.FirstName == this.FirstName && p.LastName == this.LastName;
        }
    }

    public class StubHasTenantSpecification2 : HasTenantSpecification<StubEntity> // TODO: this should be mocked
    {
        public StubHasTenantSpecification2(string tenantId)
            : base(tenantId)
        {
        }
    }

    public class StubHasTenantSpecification : Specification<StubEntity> // TODO: this should be mocked
    {
        public StubHasTenantSpecification(string tenantId)
            : base(t => t.TenantId == tenantId)
        {
        }
    }

    public class StubHasIdSpecification : Specification<StubEntity> // TODO: this should be mocked
    {
        public StubHasIdSpecification(string id)
        {
            EnsureArg.IsNotNull(id);

            this.Id = id;
        }

        public string Id { get; }

        public override Expression<Func<StubEntity, bool>> ToExpression()
        {
            return p => p.Id == this.Id;
        }
    }

    //public class StubHasNameSpecificationMapper : ISpecificationMapper<StubEntity, StubDb>
    //{
    //    public bool CanHandle(ISpecification<StubEntity> specification)
    //    {
    //        return specification.Is<StubHasNameSpecification>();
    //    }

    //    public Func<StubDb, bool> Map(ISpecification<StubEntity> specification)
    //    {
    //        var s = specification.As<StubHasNameSpecification>();
    //        return td => td.FullName == $"{s.FirstName} {s.LastName}";
    //    }
    //}

#pragma warning disable SA1204 // Static elements must appear before instance elements
    public static class StubEntityMapperConfiguration
#pragma warning restore SA1204 // Static elements must appear before instance elements
    {
        public static AutoMapper.IMapper Create()
        {
            var mapper = new AutoMapper.MapperConfiguration(c =>
            {
                // TODO: try reversemap https://stackoverflow.com/questions/13490456/automapper-bidirectional-mapping-with-reversemap-and-formember
                //c.AddExpressionMapping();
                //c.IgnoreUnmapped();
                //c.AllowNullCollections = true;
                c.CreateMap<StubEntity, StubDb>()
                    .ForMember(d => d.ExtTenantId, o => o.MapFrom(s => s.TenantId))
                    .ForMember(d => d.Identifier, o => o.MapFrom(s => s.Id))
                    .ForMember(d => d.ETag, o => o.MapFrom(s => s.IdentifierHash))
                    .ForMember(d => d.Country, o => o.MapFrom(s => s.Country))
                    //.ForMember(d => d.FullName, o => o.ResolveUsing(new FullNameResolver()))
                    .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                    .ForMember(d => d.YearOfBirth, o => o.MapFrom(new YearOfBirthResolver()));

                c.CreateMap<StubDb, StubEntity>()
                    .ForMember(d => d.TenantId, o => o.MapFrom(s => s.ExtTenantId))
                    .ForMember(d => d.Id, o => o.MapFrom(s => s.Identifier))
                    .ForMember(d => d.IdentifierHash, o => o.MapFrom(s => s.ETag))
                    .ForMember(d => d.Country, o => o.MapFrom(s => s.Country))
                    //.ForMember(d => d.FirstName, o => o.ResolveUsing(new FirstNameResolver()))
                    .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FullName.Split(' ', StringSplitOptions.None).FirstOrDefault()))
                    //.ForMember(d => d.LastName, o => o.ResolveUsing(new LastNameResolver()))
                    .ForMember(d => d.LastName, o => o.MapFrom(s => s.FullName.Split(' ', StringSplitOptions.None).LastOrDefault()))
                    .ForMember(d => d.Age, o => o.MapFrom(new AgeResolver()))
                    .ForMember(d => d.State, o => o.Ignore());

                //c.CreateMap<StubDto, ITenantEntity>()
                //    .ForMember(d => d.TenantId, o => o.MapFrom(s => s.ExtTenantId));
            });

            mapper.AssertConfigurationIsValid();
            return mapper.CreateMapper();
        }

        //private class FullNameResolver : IValueResolver<StubEntity, StubDto, string>
        //{
        //    public string Resolve(StubEntity source, StubDto destination, string destMember, ResolutionContext context)
        //    {
        //        return $"{source.FirstName} {source.LastName}";
        //    }
        //}

        private class YearOfBirthResolver : AutoMapper.IValueResolver<StubEntity, StubDb, int>
        {
            public int Resolve(StubEntity source, StubDb destination, int destMember, AutoMapper.ResolutionContext context)
            {
                return DateTime.UtcNow.Year - source.Age;
            }
        }

        //private class FirstNameResolver : IValueResolver<StubDto, StubEntity, string>
        //{
        //    public string Resolve(StubDto source, StubEntity destination, string destMember, ResolutionContext context)
        //    {
        //        return source.FullName.NullToEmpty().Split(' ').FirstOrDefault();
        //    }
        //}

        //private class LastNameResolver : IValueResolver<StubDto, StubEntity, string>
        //{
        //    public string Resolve(StubDto source, StubEntity destination, string destMember, ResolutionContext context)
        //    {
        //        return source.FullName.NullToEmpty().Split(' ').LastOrDefault();
        //    }
        //}

        private class AgeResolver : AutoMapper.IValueResolver<StubDb, StubEntity, int>
        {
            public int Resolve(StubDb source, StubEntity destination, int destMember, AutoMapper.ResolutionContext context)
            {
                return DateTime.UtcNow.Year - source.YearOfBirth;
            }
        }
    }
#pragma warning restore SA1649 // File name should match first type name
#pragma warning restore SA1402 // File may only contain a single type
}
