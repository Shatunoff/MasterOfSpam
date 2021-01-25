using System.ComponentModel;
using System.Drawing;

namespace Core
{
    public class PresentationEntity<T> : IPresentationEntity
           where T:Entity
    {
        [Browsable(false)]
        public T Entity { get; set; }

        protected string GetBoolString(bool value)
        {
            return value ? "Да" : "Нет";
        }

        public object GetEntity()
        {
            return Entity;
        }

        public virtual bool IsChecked()
        {
            return false;
        }

        public virtual void SetCheck(bool check)
        {

        }

        public virtual Color GetColor()
        {
            return Color.Empty;
        }

        public virtual Color GetFontColor()
        {
            return Color.Empty;
        }

        public virtual Color GetCellFontColor(int cellIndex)
        {
            return Color.Empty;
        }
    }

    public class PresentationClass<T> : IPresentationEntity
        where T : class
    {
        [Browsable(false)]
        public T Entity { get; set; }

        protected string GetBoolString(bool value)
        {
            return value ? "Да" : "Нет";
        }

        public object GetEntity()
        {
            return Entity;
        }

        public virtual bool IsChecked()
        {
            return false;
        }

        public virtual void SetCheck(bool check)
        {

        }

        public virtual Color GetColor()
        {
            return Color.Empty;
        }

        public virtual Color GetFontColor()
        {
            return Color.Empty;
        }

        public virtual Color GetCellFontColor(int cellIndex)
        {
            return Color.Empty;
        }
    }

    public class PresentationEnum<T> : IPresentationEntity
        where T : System.Enum
    {
        [Browsable(false)]
        public T Entity { get; set; }

        protected string GetBoolString(bool value)
        {
            return value ? "Да" : "Нет";
        }

        public object GetEntity()
        {
            return Entity;
        }

        public virtual bool IsChecked()
        {
            return false;
        }

        public virtual void SetCheck(bool check)
        {

        }

        public virtual Color GetColor()
        {
            return Color.Empty;
        }

        public virtual Color GetFontColor()
        {
            return Color.Empty;
        }

        public virtual Color GetCellFontColor(int cellIndex)
        {
            return Color.Empty;
        }
    }
}
