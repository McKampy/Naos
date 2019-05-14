﻿namespace Naos.Core.App.Web
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Naos.Core.App.Web.Controllers;
    using Naos.Core.Common;

    public class GenericRepositoryControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly IEnumerable<GenericRepositoryControllerInformation> informations;

        public GenericRepositoryControllerFeatureProvider(
            IEnumerable<GenericRepositoryControllerInformation> informations)
        {
            this.informations = informations;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            // https://www.strathweb.com/2018/04/generic-and-dynamically-generated-controllers-in-asp-net-core-mvc/

            foreach(var information in this.informations.Safe())
            {
                if(information.EntityType == null)
                {
                    continue;
                }

                if(information.RepositoryType != null)
                {
                    Type[] args = { information.EntityType, information.RepositoryType };
                    feature.Controllers.Add(typeof(NaosGenericRepositoryControllerBase<,>).MakeGenericType(args).GetTypeInfo());
                }
                else
                {
                    Type[] args = { information.EntityType };
                    feature.Controllers.Add(typeof(NaosEntityGenericRepositoryControllerBase<>).MakeGenericType(args).GetTypeInfo());
                }
            }
        }
    }
}
