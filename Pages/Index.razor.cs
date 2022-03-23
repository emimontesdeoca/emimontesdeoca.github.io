using Blog.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace Blog.Pages
{
    public partial class Index
    {
        #region Injects

        [Inject]
        public HttpClient? HttpClient { get; set; }

        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        #endregion

        #region Parameters

        [Parameter]
        public string? Path { get; set; }

        #endregion

        #region Variables

        public Post? Post { get; set; }

        public string? Content { get; set; }

        private List<Post> Posts = new();

        public bool Loaded { get; set; } = false;

        #endregion

        #region Overrides

        protected override async Task OnInitializedAsync()
        {
            Posts = await HttpClient.GetFromJsonAsync<List<Post>>("posts.json") ?? new();
            Posts?.Reverse();
        }

        protected override async Task OnParametersSetAsync()
        {
            Loaded = true;
            StateHasChanged();

            Content = string.Empty;

            if (!string.IsNullOrEmpty(Path))
            {
                Post = Posts.SingleOrDefault(x => x.Path == Path);

                if (Post == null)
                {
                    Path = string.Empty;
                    NavigationManager.NavigateTo("/");
                }
                else
                {
                    Content = await HttpClient.GetStringAsync($"/posts/{Path}.md")
                              ?? string.Empty;
                }
            }

            Loaded = true;
            StateHasChanged();
        }

        #endregion
    }
}