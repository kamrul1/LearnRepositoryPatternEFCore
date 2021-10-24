using Contracts;
using Entities;

namespace Repository
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private RepositoryContext repoContext;
        private IOwnerRepository owner;
        private IAccountRepository account;

        public IOwnerRepository Owner
        {
            get
            {
                if (owner == null)
                {
                    owner = new OwnerRepository(repoContext);
                }

                return owner;
            }
        }

        public IAccountRepository Account
        {
            get
            {
                if (account == null)
                {
                    account = new AccountRepository(repoContext);
                }

                return account;
            }
        }

        public RepositoryWrapper(RepositoryContext repositoryContext)
        {
            repoContext = repositoryContext;
        }

        public void Save()
        {
            repoContext.SaveChanges();
        }
    }
}