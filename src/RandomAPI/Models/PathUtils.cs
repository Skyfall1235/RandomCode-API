

public static class PathUtils
{
    public static string CreateOrReturnWbRootPath(IWebHostEnvironment env, string path)
    {
        return CreateOrReturnPath(env.WebRootPath, path);
    }
    public static string CreateOrReturnPath(string basePath, string path)
    {
        string newPath = Path.Combine(basePath, path);

        if (!Directory.Exists(newPath))
            Directory.CreateDirectory(newPath);
        return newPath;
    }
}
