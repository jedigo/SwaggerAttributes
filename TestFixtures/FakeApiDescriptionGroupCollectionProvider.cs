namespace SwaggerAttributes.TestFixtures
{
    using System.Buffers;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using FakeItEasy;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.DataAnnotations;
    using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Routing.Constraints;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.ObjectPool;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    public class FakeApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
    {
        private readonly List<ControllerActionDescriptor> actionDescriptors;

        public FakeApiDescriptionGroupCollectionProvider()
        {
            actionDescriptors = new List<ControllerActionDescriptor>();
        }

        public ApiDescriptionGroupCollection ApiDescriptionGroups
        {
            get
            {
                var apiDescriptions = GetApiDescriptions();
                var group = new ApiDescriptionGroup("default", apiDescriptions);
                return new ApiDescriptionGroupCollection(new[] { group }, 1);
            }
        }

        public FakeApiDescriptionGroupCollectionProvider Add<T>(string actionFixtureName)
            where T : class
        {
            actionDescriptors.Add(CreateActionDescriptor<T>(actionFixtureName));
            return this;
        }

        private IModelMetadataProvider CreateModelMetadataProvider()
        {
            var detailsProviders = new IMetadataDetailsProvider[]
            {
                new DefaultBindingMetadataProvider(),
                new DefaultValidationMetadataProvider(),
                new DataAnnotationsMetadataProvider(A.Fake<IOptions<MvcDataAnnotationsLocalizationOptions>>(), null)
            };

            var compositeDetailsProvider = new DefaultCompositeMetadataDetailsProvider(detailsProviders);
            return new DefaultModelMetadataProvider(compositeDetailsProvider);
        }

        private ControllerActionDescriptor CreateActionDescriptor<T>(string actionFixtureName)
            where T : class
        {
            var descriptor = new ControllerActionDescriptor();
            descriptor.SetProperty(new ApiDescriptionActionData());

            descriptor.ActionConstraints = new List<IActionConstraintMetadata>
            {
                new HttpMethodActionConstraint(new[] { "GET" })
            };

            descriptor.AttributeRouteInfo = new AttributeRouteInfo { Template = "testroute" };

            descriptor.MethodInfo = typeof(T).GetMethod(actionFixtureName);

            descriptor.Parameters = descriptor.MethodInfo.GetParameters()
                .Select(paramInfo => new ParameterDescriptor
                {
                    Name = paramInfo.Name,
                    ParameterType = paramInfo.ParameterType,
                    BindingInfo = BindingInfo.GetBindingInfo(paramInfo.GetCustomAttributes(false))
                })
                .ToList();

            descriptor.ControllerTypeInfo = typeof(T).GetTypeInfo();
            descriptor.FilterDescriptors = descriptor.MethodInfo.GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Select((filter) => new FilterDescriptor(filter, FilterScope.Action))
                .ToList();

            return descriptor;
        }

        private IReadOnlyList<ApiDescription> GetApiDescriptions()
        {
            var context = new ApiDescriptionProviderContext(actionDescriptors);

            var options = new MvcOptions();
            options.InputFormatters.Add(new JsonInputFormatter(A.Fake<ILogger>(), new JsonSerializerSettings(), ArrayPool<char>.Shared, new DefaultObjectPoolProvider()));
            options.OutputFormatters.Add(new JsonOutputFormatter(new JsonSerializerSettings(), ArrayPool<char>.Shared));

            var optionsAccessor = A.Fake<IOptions<MvcOptions>>();
            A.CallTo(() => optionsAccessor.Value).Returns(options);

            var constraintResolver = A.Fake<IInlineConstraintResolver>();
            A.CallTo(() => constraintResolver.ResolveConstraint("int")).Returns(new IntRouteConstraint());

            var provider = new DefaultApiDescriptionProvider(
                optionsAccessor,
                constraintResolver,
                CreateModelMetadataProvider());

            provider.OnProvidersExecuting(context);
            provider.OnProvidersExecuted(context);
            return new ReadOnlyCollection<ApiDescription>(context.Results);
        }
    }
}