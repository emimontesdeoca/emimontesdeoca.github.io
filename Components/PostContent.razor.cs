using Microsoft.AspNetCore.Components;
using Markdig;
using Microsoft.JSInterop;
//using Markdig.Prism;

namespace Blog.Components
{
    public partial class PostContent
    {
        #region Variables

        public bool Loaded { get; set; } = false;

        #endregion

        #region Injects

        [Inject]
        public IJSRuntime? JSRuntime { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        #endregion

        #region Parameters

        [Parameter, EditorRequired]
        public Blog.Models.Post? Data { get; set; }

        [Parameter, EditorRequired]
        public string? Content { get; set; }

        #endregion

        #region Methods

        public string MarkdownContent => Markdown.ToHtml(Content ?? string.Empty);
        public string GetEditUrl => $"https://github.com/emimontesdeoca/emimontesdeoca.github.io/edit/master/wwwroot/posts/{Data?.Path}.md";
        public string GetTwitterUrl => $"https://twitter.com/intent/tweet?text={Data.Title} {NavigationManager.Uri}";

        public string GetDisplayStyle => Loaded ? "display:inline" : "display:none";

        #endregion

        #region Overrides

        protected override void OnAfterRender(bool firstRender)
        {
            JSRuntime?.InvokeVoidAsync("highlightCode");
            Loaded = true;
        }

        #endregion

    }
}