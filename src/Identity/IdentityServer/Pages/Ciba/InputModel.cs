// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace IdentityServer.Pages.Ciba;

public class InputModel
{
    public string? Button { get; set; }
    public IEnumerable<string> ScopesConsented { get; set; } = Enumerable.Empty<string>();
    public string Id { get; set; } = default!;
    public string? Description { get; set; }
}