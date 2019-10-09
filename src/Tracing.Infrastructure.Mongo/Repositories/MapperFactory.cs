﻿namespace Naos.Tracing.Infrastructure.Mongo
{
    using AutoMapper;
    using Naos.Tracing.Domain;

    public static class MapperFactory
    {
        public static IMapper Create() // automapper based mapper because of the needed expression mapping
        {
            var configuration = new MapperConfiguration(c =>
            {
                //c.IgnoreUnmapped();
                c.CreateMap<LogTrace, MongoLogTrace>()
                    .ForMember(d => d.Properties.ns_ticks, o => o.MapFrom(s => s.Ticks));
                // TODO: more mappings

                c.CreateMap<MongoLogTrace, LogTrace>()
                    .ForMember(d => d.Ticks, o => o.MapFrom(s => s.Properties.ns_ticks));
                // TODO: more mappings
            });

            configuration.AssertConfigurationIsValid();
            return configuration.CreateMapper();
        }
    }
}