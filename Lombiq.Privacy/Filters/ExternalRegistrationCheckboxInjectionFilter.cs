using Lombiq.Privacy.ViewModels;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;
using System.Threading.Tasks;
using StringExtensions = OrchardCore.Modules.StringExtensions;

namespace Lombiq.Privacy.Filters;

public class ExternalRegistrationCheckboxInjectionFilter : IAsyncResultFilter
{
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IShapeFactory _shapeFactory;

    public ExternalRegistrationCheckboxInjectionFilter(
        ILayoutAccessor layoutAccessor,
        IShapeFactory shapeFactory)
    {
        _layoutAccessor = layoutAccessor;
        _shapeFactory = shapeFactory;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var routeValues = context.ActionDescriptor.RouteValues;
        if (context.IsNotFullViewRendering() ||
            !StringExtensions.EqualsOrdinalIgnoreCase(routeValues["Area"], $"{nameof(OrchardCore)}.{nameof(OrchardCore.Users)}") ||
            !StringExtensions.EqualsOrdinalIgnoreCase(routeValues["Controller"], typeof(ExternalAuthenticationsController).ControllerName()) ||
            (!StringExtensions.EqualsOrdinalIgnoreCase(routeValues["Action"], nameof(ExternalAuthenticationsController.ExternalLoginCallback)) &&
            !StringExtensions.EqualsOrdinalIgnoreCase(routeValues["Action"], nameof(ExternalAuthenticationsController.RegisterExternalLogin))))
        {
            await next();
            return;
        }

        var layout = await _layoutAccessor.GetLayoutAsync();
        var afterRegisterZone = layout.Zones["AfterRegister"];
        var shape = await _shapeFactory.CreateAsync<PrivacyRegistrationConsentCheckboxViewModel>(
            "Lombiq_Privacy_RegistrationCheckbox",
            viewModel => viewModel.RegistrationCheckbox = false);

        await afterRegisterZone.AddAsync(shape);

        await next();
    }
}
