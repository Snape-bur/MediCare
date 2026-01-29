// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration; // Added for appsettings access
using System.Net.Http; // Added for Cloudflare API call
using System.Text.Json; // Added for parsing response
using MediCare.Models;

namespace MediCare.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IConfiguration _configuration; // Added configuration

        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger,
            IConfiguration configuration) // Added to constructor
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // Property to pass SiteKey to the Razor Page
        public string TurnstileSiteKey => _configuration["CloudflareTurnstile:SiteKey"];

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // 1. Get Turnstile token and Secret Key
            var turnstileResponse = Request.Form["cf-turnstile-response"];
            var secretKey = _configuration["CloudflareTurnstile:SecretKey"];

            // 2. Security Check: Verify Turnstile
            if (string.IsNullOrEmpty(turnstileResponse) || !await VerifyTurnstileToken(turnstileResponse, secretKey))
            {
                ModelState.AddModelError(string.Empty, "Security verification failed. Please try again.");
                return Page();
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("New Patient account created.");

                    await _userManager.AddToRoleAsync(user, "Patient");
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Step1", "Registration", new { area = "Patient" });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        // Helper Method to verify the token with Cloudflare
        private async Task<bool> VerifyTurnstileToken(string token, string secretKey)
        {
            try
            {
                using var client = new HttpClient();
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", secretKey),
                    new KeyValuePair<string, string>("response", token)
                });

                var response = await client.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Parse JSON to check success
                using var doc = JsonDocument.Parse(jsonResponse);
                return doc.RootElement.GetProperty("success").GetBoolean();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Turnstile verification error: {ex.Message}");
                return false;
            }
        }

        private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not abstract and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}