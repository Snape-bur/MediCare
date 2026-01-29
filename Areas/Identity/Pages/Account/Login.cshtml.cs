// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MediCare.Models;

namespace MediCare.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LoginModel(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            ILogger<LoginModel> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // 🔑 Expose Site Key to Razor
        public string RecaptchaSiteKey =>
            _configuration["Captcha:Recaptcha:SiteKey"];

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins =
                (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins =
                (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // 🔐 1. reCAPTCHA validation (FIRST)
            var captchaResponse = Request.Form["g-recaptcha-response"];

            if (string.IsNullOrEmpty(captchaResponse))
            {
                ModelState.AddModelError(string.Empty, "Please verify that you are not a robot.");
                return Page();
            }

            var secretKey = _configuration["Captcha:Recaptcha:SecretKey"];
            var client = _httpClientFactory.CreateClient();

            var verifyResponse = await client.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "secret", secretKey },
                    { "response", captchaResponse },
                    { "remoteip", HttpContext.Connection.RemoteIpAddress?.ToString() }
                })
            );

            var json = await verifyResponse.Content.ReadAsStringAsync();
            var captchaResult =
                JsonSerializer.Deserialize<RecaptchaResponse>(json);

            if (captchaResult == null || !captchaResult.success)
            {
                ModelState.AddModelError(string.Empty, "Captcha verification failed.");
                return Page();
            }

            // 🔐 2. Model validation
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 🔐 3. Login
            var result = await _signInManager.PasswordSignInAsync(
                Input.Email,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                if (await _userManager.IsInRoleAsync(user, "Doctor"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Doctor" });

                if (await _userManager.IsInRoleAsync(user, "Patient"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Patient" });

                return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor)
                return RedirectToPage("./LoginWith2fa",
                    new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });

            if (result.IsLockedOut)
                return RedirectToPage("./Lockout");

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        // 🔐 Helper class
        private class RecaptchaResponse
        {
            public bool success { get; set; }
        }
    }
}
