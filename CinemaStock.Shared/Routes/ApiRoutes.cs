namespace CinemaStock.Shared.Routes;

public class ApiRoutes
{
    private const string BaseApi = "api";

    public static class Account
    {
        public const string Base = $"{BaseApi}/account";

        public const string RegisterEndpoint = "register";
        public const string LoginEndpoint = "login";
        public const string LogoutEndpoint = "logout";
        public const string ProfileEndpoint = "profile";
        public const string CurrentUserEndpoint = "current";

        public const string Register = $"{Base}/{RegisterEndpoint}";
        public const string Login = $"{Base}/{LoginEndpoint}";
        public const string CurrentUser = $"{Base}/{CurrentUserEndpoint}";
        public const string UserLogout = $"{Base}/{LogoutEndpoint}";
    }

    public static class Admin
    {
        #region Cinema Content

        public const string GetCinemaEndpoint = "get-cinemas";
        public const string GetCinemaSearchLiveEndpoint = "get-cinemas-search-live";
        public const string CreateCinemaEndpoint = "create-cinema";
        public const string UpdateCinemaEndpoint = "update-cinema";
        public const string DeleteCinemaEndpoint = "delete-cinema";

        public const string CinemaContentEndpoint = $"{BaseApi}/cinema-content";
        
        public const string GetCinemaContent = $"{CinemaContentEndpoint}/{GetCinemaEndpoint}";
        public const string GetCinemaSearchLive = $"{CinemaContentEndpoint}/{GetCinemaSearchLiveEndpoint}";
        public const string CreateCinemaContent = $"{CinemaContentEndpoint}/{CreateCinemaEndpoint}";
        public const string UpdateCinemaContent = $"{CinemaContentEndpoint}/{UpdateCinemaEndpoint}";
        public const string DeleteCinemaContent = $"{CinemaContentEndpoint}/{DeleteCinemaEndpoint}";

        #endregion

        #region Genres and Tags

        public const string ListEndpoint = "list";

        public const string GenresControllerName = $"{BaseApi}/genres";
        public const string GenresList = $"{GenresControllerName}/{ListEndpoint}";
        public const string TagsControllerName = $"{BaseApi}/tags";
        public const string TagsList = $"{TagsControllerName}/{ListEndpoint}";

        #endregion
    }
}