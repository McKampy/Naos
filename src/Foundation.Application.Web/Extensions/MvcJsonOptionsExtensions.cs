﻿//namespace Naos.Foundation.Application
//{
//    using Microsoft.AspNetCore.Mvc;
//    using Newtonsoft.Json;

//    public static class MvcJsonOptionsExtensions
//    {
//        public static MvcJsonOptions AddDefaultJsonSerializerSettings(
//            this MvcJsonOptions source,
//            JsonSerializerSettings settings = null)
//        {
//            settings ??= DefaultJsonSerializerSettings.Create();

//            // copy some json serializer settings for the mvcoptions
//            source.SerializerSettings.ContractResolver = settings.ContractResolver;
//            source.SerializerSettings.Converters = settings.Converters;
//            source.SerializerSettings.DefaultValueHandling = settings.DefaultValueHandling;
//            source.SerializerSettings.NullValueHandling = settings.NullValueHandling;

//            return source;
//        }
//    }
//}
