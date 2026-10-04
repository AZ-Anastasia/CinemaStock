namespace CinemaStock.Shared.Resources.UserErrors;

public class UserErrors : BaseResource
{
    public static string BadFileFormat => GetValue("BadFileFormat");
    public static string TooLargeFileSize => GetValue("TooLargeFileSize");
    public static string ErrorFileFormat => GetValue("ErrorFileFormat");

    public static string ServerConnectionFailed => GetValue("ServerConnectionFailed");
    public static string BadLoginOrPassword => GetValue("BadLoginOrPassword");
    public static string AccountBlocked => GetValue("AccountBlocked");
    public static string LoginIsNotAllowed => GetValue("LoginIsNotAllowed");
    public static string TwoFactorAuthRequired => GetValue("TwoFactorAuthRequired");

    public static string TagsReadingError => GetValue("TagsReadingError");
    public static string TagsPostingError => GetValue("TagsPostingError");
    public static string TagsUpdatingError => GetValue("TagsUpdatingError");
    public static string TagsDeletingError => GetValue("TagsDeletingError");
    public static string TagNameRequired => GetValue("TagNameRequired");
    public static string TagAlreadyExist => GetValue("TagAlreadyExist");

    public static string GenreReadingError => GetValue("GenreReadingError");
    public static string GenrePostingError => GetValue("GenrePostingError");
    public static string GenreUpdatingError => GetValue("GenreUpdatingError");
    public static string GenreDeletingError => GetValue("GenreDeletingError");
    public static string GenreNameRequired => GetValue("GenreNameRequired");
    public static string GenreAlreadyExist => GetValue("GenreAlreadyExist");

    public static string ReadingError => GetValue("ReadingError");
    public static string UpdatingError => GetValue("UpdatingError");
    public static string DeletingError => GetValue("DeletingError");
    public static string EmptyBodyError => GetValue("EmptyBodyError");
    public static string PostError => GetValue("PostError");
    public static string NoContentInDB => GetValue("NoContentInDB");
    public static string RequestQueryLengthError => GetValue("RequestQueryLengthError");
}