using Flyrank.WidgetPlatform.Api.Data;
using Flyrank.WidgetPlatform.Api.Domain.Models;
using System.Text.Encodings.Web;

namespace Flyrank.WidgetPlatform.Api.Services;

public sealed class WidgetService(IWidgetPlatformStore store) : IWidgetService
{
    public IReadOnlyList<WidgetResponse> GetAll(Guid tenantId) =>
        store.GetWidgets(tenantId).Select(ToResponse).ToArray();

    public WidgetResponse? GetById(Guid tenantId, Guid widgetId) =>
        store.GetWidget(tenantId, widgetId) is { } widget ? ToResponse(widget) : null;

    public WidgetResponse Create(CreateWidgetRequest request)
    {
        var widget = new Widget(
            Guid.NewGuid(),
            request.TenantId,
            request.Name.Trim(),
            request.Type.Trim(),
            request.Title.Trim(),
            request.ButtonText.Trim(),
            DateTimeOffset.UtcNow);

        return ToResponse(store.AddWidget(widget));
    }

    public WidgetResponse? Update(Guid widgetId, UpdateWidgetRequest request)
    {
        var existing = store.GetWidget(request.TenantId, widgetId);
        if (existing is null)
        {
            return null;
        }

        var updated = existing with
        {
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            Title = request.Title.Trim(),
            ButtonText = request.ButtonText.Trim()
        };

        return store.UpdateWidget(updated) ? ToResponse(updated) : null;
    }

    public bool Delete(Guid tenantId, Guid widgetId) => store.DeleteWidget(tenantId, widgetId);

    public WidgetConfigurationResponse? GetConfiguration(Guid widgetId) =>
        store.GetWidgetById(widgetId) is { } widget
            ? new WidgetConfigurationResponse(widget.Id, widget.Type, widget.Title, widget.ButtonText)
            : null;

    public WidgetEmbedResponse? GetEmbed(Guid widgetId, string? publicBaseUrl)
    {
        if (!store.WidgetExists(widgetId))
        {
            return null;
        }

        var baseUrl = (publicBaseUrl ?? "https://localhost").TrimEnd('/');
        var encodedBaseUrl = HtmlEncoder.Default.Encode(baseUrl);
        return new WidgetEmbedResponse(
            $"<script src=\"{encodedBaseUrl}/api/widgets/{widgetId:D}/loader.js\" async></script>");
    }

    public string? GetLoaderScript(Guid widgetId, string? publicBaseUrl)
    {
        if (!store.WidgetExists(widgetId))
        {
            return null;
        }

        var baseUrl = System.Text.Json.JsonSerializer.Serialize(
            (publicBaseUrl ?? "https://localhost").TrimEnd('/'));
        var widgetIdValue = widgetId.ToString("D");

        return $$"""
            (() => {
              const apiBase = {{baseUrl}};
              const widgetId = "{{widgetIdValue}}";
              const script = document.currentScript;
              if (!script) return;
              fetch(`${apiBase}/api/widgets/${widgetId}/configuration`)
                .then(response => {
                  if (!response.ok) throw new Error("Widget configuration could not be loaded.");
                  return response.json();
                })
                .then(config => {
                  const root = document.createElement("section");
                  root.setAttribute("aria-label", config.title);
                  const title = document.createElement("h2");
                  title.textContent = config.title;
                  const form = document.createElement("form");
                  const email = document.createElement("input");
                  email.type = "email";
                  email.required = true;
                  email.autocomplete = "email";
                  email.setAttribute("aria-label", "Email");
                  const website = document.createElement("input");
                  website.type = "text";
                  website.name = "website";
                  website.tabIndex = -1;
                  website.autocomplete = "off";
                  website.setAttribute("aria-hidden", "true");
                  website.style.display = "none";
                  const button = document.createElement("button");
                  button.type = "submit";
                  button.textContent = config.buttonText;
                  const status = document.createElement("p");
                  status.setAttribute("role", "status");
                  form.append(email, website, button);
                  root.append(title, form, status);
                  script.insertAdjacentElement("afterend", root);
                  form.addEventListener("submit", async event => {
                    event.preventDefault();
                    button.disabled = true;
                    try {
                      const result = await fetch(`${apiBase}/api/submissions`, {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({
                          widgetId,
                          visitorData: { email: email.value },
                          website: website.value
                        })
                      });
                      if (!result.ok) throw new Error("Submission failed.");
                      status.textContent = "Thank you. Your submission was received.";
                      form.reset();
                    } catch {
                      status.textContent = "Unable to submit right now. Please try again.";
                    } finally {
                      button.disabled = false;
                    }
                  });
                })
                .catch(error => console.error(error));
            })();
            """;
    }

    private static WidgetResponse ToResponse(Widget widget) =>
        new(widget.Id, widget.TenantId, widget.Name, widget.Type, widget.Title, widget.ButtonText, widget.CreatedAt);
}
