Even wondering how Microsoft name stuff so rare? I've always thouth that they don't really do it that great but, well that's what it is!

The good thing about this is that you can pretty much can change everything while you are developing!

Have you ever gotten into a page and you instantly realize this is an ASP.NET Web project just by doing the register or login process? 

<img src="https://imgur.com/8NMNHGp.png">

It has happened to me a lot because by default in the project you create, you have these urls to do the login and register process(also you have a lot of other pages too).

So this tutorial shows a way to update those URLs so the look nicer on your project!!

## Default behavior

When we are creating a Blazor project and we decide to use it with Identity, it displays something like this:

<img align="center" src="https://i.imgur.com/2W8Oou9.png">

And the when we try to do the login or register process, we go either `/Identity/Account/Login` or `/Identity/Account/Register`.

But what if I tell you that you can acutally update those URLs to be different?

## Scaffolding the `Login ` and `Register` pages

In order to update these pages, Microsoft hides them, but you can scaffold them quickly and do the changes you want!

In order to do that you need to go and `Add Scaffolded Item` in the context menu of the project, just like this:

<img src="https://imgur.com/F3C4C9b.png">

Then it will popup a modal and you have to select `Identity` twice and click on `Add`:

<img src="https://imgur.com/tZUqUlY.png">

After this one, it will popup yet another modal, where you can select which pages from the entire Identity you want to update. There's a lot of pages that you can update, but we're going to focus on `Account/Login` and `Account/Register`:

<img src="https://imgur.com/BS6ZLas.png">

Now let it work for a little bit and then check the Solution Explorer, you'll find a few new files:

<img src="https://imgur.com/5lWHLyI.png">

These new files are the login and register page that ASP.NET adds to your project when you select it to add Identity!

## Updating urls

As you've probably noticed, this files are razor files, since their extension is `cshtml`, so we're just going to use a directive to update the page's url:

```csharp
@page "/login"
@model LoginModel

@{
    ViewData["Title"] = "Log in";
}

<h1>@ViewData["Title"]</h1>
<div class="row">
    <div class="col-md-4">
        <section>
            <form id="account" method="post">
                <h2>Use a local account to log in.</h2>
                <hr />
                <div asp-validation-summary="ModelOnly" class="text-danger" role="alert"></div>
                <div class="form-floating mb-3">
                    <input asp-for="Input.Email" class="form-control" autocomplete="username" aria-required="true" placeholder="name@example.com" />
                    <label asp-for="Input.Email" class="form-label">Email</label>
                    <span asp-validation-for="Input.Email" class="text-danger"></span>
                </div>
                <div class="form-floating mb-3">
                    <input asp-for="Input.Password" class="form-control" autocomplete="current-password" aria-required="true" placeholder="password" />
                    <label asp-for="Input.Password" class="form-label">Password</label>
                    <span asp-validation-for="Input.Password" class="text-danger"></span>
                </div>
                <div class="checkbox mb-3">
                    <label asp-for="Input.RememberMe" class="form-label">
                        <input class="form-check-input" asp-for="Input.RememberMe" />
                        @Html.DisplayNameFor(m => m.Input.RememberMe)
                    </label>
                </div>
                <div>
                    <button id="login-submit" type="submit" class="w-100 btn btn-lg btn-primary">Log in</button>
                </div>
                <div>
                    <p>
                        <a id="forgot-password" asp-page="./ForgotPassword">Forgot your password?</a>
                    </p>
                    <p>
                        <a asp-page="./Register" asp-route-returnUrl="@Model.ReturnUrl">Register as a new user</a>
                    </p>
                    <p>
                        <a id="resend-confirmation" asp-page="./ResendEmailConfirmation">Resend email confirmation</a>
                    </p>
                </div>
            </form>
        </section>
    </div>
    <div class="col-md-6 col-md-offset-2">
        <section>
            <h3>Use another service to log in.</h3>
            <hr />
            @{
                if ((Model.ExternalLogins?.Count ?? 0) == 0)
                {
                    <div>
                        <p>
                            There are no external authentication services configured. See this <a href="https://go.microsoft.com/fwlink/?LinkID=532715">article
                            about setting up this ASP.NET application to support logging in via external services</a>.
                        </p>
                    </div>
                }
                else
                {
                    <form id="external-account" asp-page="./ExternalLogin" asp-route-returnUrl="@Model.ReturnUrl" method="post" class="form-horizontal">
                        <div>
                            <p>
                                @foreach (var provider in Model.ExternalLogins!)
                                {
                                    <button type="submit" class="btn btn-primary" name="provider" value="@provider.Name" title="Log in using your @provider.DisplayName account">@provider.DisplayName</button>
                                }
                            </p>
                        </div>
                    </form>
                }
            }
        </section>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

As you can see most of the stuff is the same, but if you take a look at the very first line in the class, I've updated what we had before from:

```csharp
@page
```

to

```csharp
@page "/login"
```

Well, that was easy, wasn't it? Now let's do a quick test to see if it works.

First of all, let's go to the default page that we have at first, to check if that's still working:

<img src="https://imgur.com/ngbRNaG.png">

Which it doesn't! So now let's go and test our new url which is `/login`:

<img src="https://imgur.com/R067PnF.png">

And it works!!

Now let's do the same for the register, we update its page and add the path we want in the `@page` directive and let's do a test!

```csharp
@page "/register"
@model RegisterModel
@{
    ViewData["Title"] = "Register";
}

<h1>@ViewData["Title"]</h1>

<div class="row">
    <div class="col-md-4">
        <form id="registerForm" asp-route-returnUrl="@Model.ReturnUrl" method="post">
            <h2>Create a new account.</h2>
            <hr />
            <div asp-validation-summary="ModelOnly" class="text-danger" role="alert"></div>
            <div class="form-floating mb-3">
                <input asp-for="Input.Email" class="form-control" autocomplete="username" aria-required="true" placeholder="name@example.com" />
                <label asp-for="Input.Email">Email</label>
                <span asp-validation-for="Input.Email" class="text-danger"></span>
            </div>
            <div class="form-floating mb-3">
                <input asp-for="Input.Password" class="form-control" autocomplete="new-password" aria-required="true" placeholder="password" />
                <label asp-for="Input.Password">Password</label>
                <span asp-validation-for="Input.Password" class="text-danger"></span>
            </div>
            <div class="form-floating mb-3">
                <input asp-for="Input.ConfirmPassword" class="form-control" autocomplete="new-password" aria-required="true" placeholder="password" />
                <label asp-for="Input.ConfirmPassword">Confirm Password</label>
                <span asp-validation-for="Input.ConfirmPassword" class="text-danger"></span>
            </div>
            <button id="registerSubmit" type="submit" class="w-100 btn btn-lg btn-primary">Register</button>
        </form>
    </div>
    <div class="col-md-6 col-md-offset-2">
        <section>
            <h3>Use another service to register.</h3>
            <hr />
            @{
                if ((Model.ExternalLogins?.Count ?? 0) == 0)
                {
                    <div>
                        <p>
                            There are no external authentication services configured. See this <a href="https://go.microsoft.com/fwlink/?LinkID=532715">article
                            about setting up this ASP.NET application to support logging in via external services</a>.
                        </p>
                    </div>
                }
                else
                {
                    <form id="external-account" asp-page="./ExternalLogin" asp-route-returnUrl="@Model.ReturnUrl" method="post" class="form-horizontal">
                        <div>
                            <p>
                                @foreach (var provider in Model.ExternalLogins!)
                                {
                                    <button type="submit" class="btn btn-primary" name="provider" value="@provider.Name" title="Log in using your @provider.DisplayName account">@provider.DisplayName</button>
                                }
                            </p>
                        </div>
                    </form>
                }
            }
        </section>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

I've updated the top part with `@page "/login"` and now we test if it works:

<img src="https://imgur.com/M7KakaF.png">

Also works!!

## That's it

I hope you learned how to update this urls, mostly because on some projects when you're doing the urls a certain way and then the Identity looks different, it sucks haha!

If you need anything just tweet at me or send me an email and I'll try to give a hand!