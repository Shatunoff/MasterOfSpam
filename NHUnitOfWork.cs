using NHibernate;
using System;
using System.Collections.Generic;

namespace Core.NH
{
    public class NHUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Текущая сессия (контекст пользователя)
        /// </summary>
        public ISession Session { get { return SessionAccessor.GetAbsSession(); } }

        private List<Entity> newItems = new List<Entity>();
        private List<Entity> dirtyItems = new List<Entity>();
        private List<Entity> deletedItems = new List<Entity>();
        private List<Entity> ignoreItems = new List<Entity>();
        private List<IUnitOfWorkAction> actions = new List<IUnitOfWorkAction>();

        public IEnumerable<Entity> NewItems
        {
            get
            {
                return newItems;
            }
        }

        public NHUnitOfWork()
        {
        }

        #region IUnitOfWork Members

        public void Ignore(Entity item)
        {
            if (!ignoreItems.Contains(item))
                ignoreItems.Add(item);
        }

        public void RegisterNew(Entity item)
        {
            if (item != null)
            {
                if (!item.IsNew())
                {
                     RegisterDirty(item);
                }
                else
                {
                    if (!newItems.Contains(item))
                        newItems.Add(item);
                }
            }
        }

        public void RegisterDirty(Entity item)
        {
            if (item != null)
            {
                if (item.IsNew())
                {
                    RegisterNew(item);
                }
                else
                {
                    // чтобы не добавить объект в список для обновления, который еще не закоммитили
                    if (!newItems.Contains(item))
                    {
                        if (!dirtyItems.Contains(item))
                            dirtyItems.Add(item);
                    }
                }
            }
        }

        public void RegisterDirtyRef(Entity item)
        {
            if (item != null)
            {
                if (item.IsNew())
                {
                    RegisterNew(item);
                    return;
                }
                if (item != null)
                {
                    if (!dirtyItems.Contains(item))
                        dirtyItems.Add(item);
                }
            }
        }

        public void RegisterDeleted(Entity item)
        {
            if (newItems.Contains(item))
                newItems.Remove(item);
            else if (!deletedItems.Contains(item) && !item.IsNew())
            {
                if (dirtyItems.Contains(item))
                    dirtyItems.Remove(item);
                deletedItems.Add(item);
            }
        }

        public void RegisterAction(IUnitOfWorkAction item)
        {
            if (!actions.Contains(item))
                actions.Add(item);
        }

        public void RecreateSession()
        {
            SessionAccessor.GetAbsSession();
        }
        
        public void Commit()
        {
            if (newItems.Count > 0 || dirtyItems.Count > 0 || deletedItems.Count > 0)
            {
                if (!Session.IsOpen) RecreateSession();

                SessionAccessor.InitContext(Session);

                using (var transaction = Session.BeginTransaction())
                {
                    foreach (Entity deletedObject in deletedItems)
                    {
                        Session.Delete(deletedObject);
                    }
                    foreach (Entity newObject in newItems)
                    {
                        newObject.OnSave(this);
                        Session.Save(newObject);
                    }
                    foreach (Entity dirtyObject in dirtyItems)
                    {
                        dirtyObject.OnSave(this);
                        if (!ignoreItems.Contains(dirtyObject)) Session.SaveOrUpdate(dirtyObject);
                    }

                    try
                    {
                        Session.Flush();

                        foreach (var action in actions)
                        {
                            action.Execute();
                        }

                        transaction.Commit();
                        ClearList();
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError(ex, "", "NHUnitOfWork.Commit()");
                        throw;
                    }
                }
            }
        }

        private void ClearList()
        {
            newItems = new List<Entity>();
            dirtyItems = new List<Entity>();
            deletedItems = new List<Entity>();
            actions = new List<IUnitOfWorkAction>();
        }

        public void Rollback()
        {
            foreach (Entity dirtyObject in dirtyItems)
            {
                Session.Refresh(dirtyObject);
            }

            foreach (var item in actions)
            {
                item.Rollback();
            }

            ClearList();
        }

        public void Init()
        {
        }

        public void Refresh(Entity entity)
        {
            Session.Refresh(entity);
        }

        #endregion

        #region IDisposable Members

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Rollback();
                }
            }
            disposed = true;
        }

        #endregion
    }
}
