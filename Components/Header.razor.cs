using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Blog;
using Blog.Shared;
using Microsoft.Extensions.Configuration;

namespace Blog.Components
{
    public partial class Header
    {
        [Inject]
        public IJSRuntime? JSRuntime { get; set; } = default;
        public bool IsDarkMode { get; set; } = false;

        public async Task ToggleDarkMode()
        {
            var theme = await JSRuntime!.InvokeAsync<string>("toggleTheme");
            IsDarkMode = theme == "dark";
        }

        protected override async Task OnInitializedAsync()
        {
            await JSRuntime!.InvokeVoidAsync("loadTheme");
            var value = (await JSRuntime!.InvokeAsync<string>("getTheme") == "dark");
            IsDarkMode =value;
        }
    }
}