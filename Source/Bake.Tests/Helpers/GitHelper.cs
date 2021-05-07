using System;
using LibGit2Sharp;

namespace Bake.Tests.Helpers
{
    public class GitHelper
    {
        public static void Create(string path)
        {
            Repository.Init(path);
            
            using var repository = new Repository(path);

            repository.Network.Remotes.Add("origin", "https://github.com/rasmus/Bake.git");

            var signature = new Signature("test", "test@example.org", DateTimeOffset.Now);
            repository.Commit(
                "test",
                signature,
                signature,
                new CommitOptions
                {
                    AllowEmptyCommit = true,
                });
        }
    }
}
