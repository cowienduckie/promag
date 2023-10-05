// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Account.Login;

public class InputModel
{
    [Required]
    public string Username { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

    public bool RememberLogin { get; set; }

    public string ReturnUrl { get; set; } = default!;

    public string Button { get; set; } = default!;
}