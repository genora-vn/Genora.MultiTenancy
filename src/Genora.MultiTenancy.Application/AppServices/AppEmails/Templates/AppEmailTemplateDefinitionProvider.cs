using Genora.MultiTenancy.Features.AppEmails;
using Volo.Abp.TextTemplating;
using Volo.Abp.TextTemplating.Scriban;

namespace Genora.MultiTenancy.AppServices.AppEmails.Templates;

public class AppEmailTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(
            new TemplateDefinition(AppEmailTemplateNames.BookingNewRequest)
                .WithScribanEngine()
                .WithVirtualFilePath(
                    "/AppServices/AppEmails/Templates/BookingNewRequest.tpl",
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(AppEmailTemplateNames.BookingChangeRequest)
                .WithScribanEngine()
                .WithVirtualFilePath(
                    "/AppServices/AppEmails/Templates/BookingChangeRequest.tpl",
                    isInlineLocalized: true
                )
        );
        
        context.Add(
            new TemplateDefinition(AppEmailTemplateNames.BookingCancelRequest)
                .WithScribanEngine()
                .WithVirtualFilePath(
                    "/AppServices/AppEmails/Templates/BookingCancelRequest.tpl",
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(AppEmailTemplateNames.OrderProductRequest)
                .WithScribanEngine()
                .WithVirtualFilePath(
                    "/AppServices/AppEmails/Templates/OrderProductRequest.tpl",
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(AppEmailTemplateNames.FnbOrderNewRequest)
                .WithScribanEngine()
                .WithVirtualFilePath(
                    "/AppServices/AppEmails/Templates/FnbOrderNewRequest.tpl",
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(AppEmailTemplateNames.ProshopOrderNewRequest)
                .WithScribanEngine()
                .WithVirtualFilePath(
                    "/AppServices/AppEmails/Templates/ProshopOrderNewRequest.tpl",
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(AppEmailTemplateNames.CaddieBookingNewRequest)
                .WithScribanEngine()
                .WithVirtualFilePath(
                    "/AppServices/AppEmails/Templates/CaddieBookingNewRequest.tpl",
                    isInlineLocalized: true
                )
        );
    }
}