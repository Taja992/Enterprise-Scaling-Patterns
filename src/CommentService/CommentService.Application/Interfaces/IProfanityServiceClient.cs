namespace CommentService.Application.Interfaces;

public interface IProfanityServiceClient
{
    Task<bool> ContainsProfanityAsync(string text);
}
