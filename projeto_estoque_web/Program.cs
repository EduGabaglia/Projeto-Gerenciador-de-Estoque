using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(field => $"O campo \"{field}\" é obrigatório.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, field) => $"O valor \"{value}\" não é válido para \"{field}\".");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(field => $"O campo \"{field}\" é obrigatório.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => "Informe um valor.");
    options.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(() => "Corpo da requisição é obrigatório.");
    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"O valor \"{value}\" não é válido.");
    options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor informado não é válido.");
    options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(() => "O campo deve ser um número.");
    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(field => $"O valor informado para \"{field}\" não é válido.");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(field => $"O valor informado para \"{field}\" não é válido.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(field => $"O campo \"{field}\" deve ser um número.");
});

var app = builder.Build();

var culturaPtBr = new CultureInfo("pt-BR");

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaPtBr),
    SupportedCultures = new[] { culturaPtBr },
    SupportedUICultures = new[] { culturaPtBr }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
