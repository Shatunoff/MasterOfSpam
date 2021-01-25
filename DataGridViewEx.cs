using Core;
using Gizmox.WebGUI.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace Web.Controls
{
    public class DataGridViewItemFormatting : EventArgs
    {
        public object Item { get; set; }
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }
    }

    public class DataGridViewEx : DataGridView
    {
        public DataGridViewEx()
        {
            RowHeadersDefaultCellStyle.Font = new Font("Tahoma", 10F);
            Font = new Font("Tahoma", 10F);
            BackgroundColor = Color.White;
            RowHeadersVisible = false;
            ShowEditingIcon = false;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            EditMode = DataGridViewEditMode.EditProgrammatically;
            MultiSelect = false;
            ReadOnly = true;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            EnableSort = true;
            ItemsPerPage = 50;
            RowTemplate.Height = 23;
            RowTemplate.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            Resize += new EventHandler(DataGridViewEx_Resize);
            CellFormatting += new DataGridViewCellFormattingEventHandler(DataGridViewEx_CellFormatting);
            CellClick += new DataGridViewCellEventHandler(DataGridViewEx_CellClick);
            DataSourceChanged += new EventHandler(DataGridViewEx_DataSourceChanged);
            PagingChanged += DataGridViewEx_PagingChanged;
            //RowEnter += new DataGridViewCellEventHandler(DataGridViewEx_RowEnter);
        }

        /*private void DataGridViewEx_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (Rows == null || Rows.Count == 0) return;

            if (Rows[e.RowIndex].DataBoundItem is IPresentationEntity item)
            {
                var color = item.GetColor();

                // Цвет активной строки на тон темнее
                RowsDefaultCellStyle.SelectionBackColor = (!color.IsEmpty)
                    ? Color.FromArgb(color.A, (byte)(color.R / 1.1), (byte)(color.G / 1.1), (byte)(color.B / 1.1))
                    : Color.Empty;
            }
        }*/

        private void DataGridViewEx_PagingChanged(object sender, EventArgs e)
        {
            RefreshView();
        }

        void DataGridViewEx_DataSourceChanged(object sender, EventArgs e)
        {
            propertyCache.Clear();
            propertyFormatCache.Clear();
            AutoSizeColumnsWidth();
            RefreshView();
        }

        void RefreshView()
        {
            AutoSizeRowsHeight();

            if (ItemFormatting != null && Rows.Count > 0)
            {
                for (int i = (CurrentPage * ItemsPerPage) - ItemsPerPage; i <= CurrentPage * ItemsPerPage; i++)
                {
                    if (i < Rows.Count)
                        foreach (DataGridViewCell cell in Rows[i].Cells)
                        {
                            if (i >= 0)
                            {
                                ItemFormatting(this, new DataGridViewItemFormatting()
                                {
                                    Item = Rows[i].DataBoundItem,
                                    ColumnIndex = cell.ColumnIndex,
                                    RowIndex = i
                                });
                            }
                        }
                }
            }
            Update();
            UpdateColors();
        }

        private void UpdateColors()
        {
            if (Rows == null || Rows.Count == 0) return;

            for (int iRow = (CurrentPage * ItemsPerPage) - ItemsPerPage; iRow <= CurrentPage * ItemsPerPage; iRow++)
            {
                if (iRow < Rows.Count && Rows[iRow].DataBoundItem is IPresentationEntity item)
                {
                    if (item != null)
                    {
                        var color = item.GetColor();
                        SetRowColor(iRow, color);

                        var fontColor = item.GetFontColor();
                        SetRowFontColor(iRow, fontColor);

                        for (int iCol = 0; iCol < Rows[iRow].Cells.Count; iCol++)
                        {
                            var cellFontColor = item.GetCellFontColor(iCol);
                            SetCellFontColor(iRow, iCol, cellFontColor);
                        }
                    }
                }
            }
        }

        public void SetRowColor(int index, Color color)
        {
            Rows[index].DefaultCellStyle.BackColor = color;
        }

        public void SetCellFontColor(int rowIndex, int cellIndex, Color color)
        {
            Rows[rowIndex].Cells[cellIndex].Style.ForeColor = color;
        }

        public void SetRowFontColor(int index, Color color)
        {
            Rows[index].DefaultCellStyle.ForeColor = color;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EnableSort { get; set; }

        public event EventHandler<DataGridViewItemFormatting> ItemFormatting = delegate { };

        /// <summary>
        /// Клик на заголовке столбца
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DataGridViewEx_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!EnableSort || e.RowIndex >= 0) return;

            var column = Columns[e.ColumnIndex];

            if (column.SortMode == DataGridViewColumnSortMode.NotSortable)
            {
                ClearTags(null);
                return;
            }

            InitProp(e.ColumnIndex);

            if (!propertyCache.ContainsKey(column.DataPropertyName)) return;

            var property = propertyCache[column.DataPropertyName];

            var t = typeof(PropertyComparer<>).MakeGenericType(GetItemType());

            var method = DataSource.GetType().GetMethod("Sort", new[] { t });
            var comparer = Activator.CreateInstance(t, new object[] { property, GetNextOrder(column) });
            ClearTags(column);

            if (method == null) return;
            method.Invoke(DataSource, new[] { comparer });

            column.HeaderCell.SortGlyphDirection = ((PropertyOrder)column.Tag == PropertyOrder.Asc) ? SortOrder.Ascending : SortOrder.Descending;

            UpdateColors();
        }

        private PropertyOrder GetNextOrder(DataGridViewColumn column)
        {
            if (column.Tag == null)
            {
                column.Tag = PropertyOrder.Asc;
            }
            else if ((PropertyOrder)column.Tag == PropertyOrder.Asc)
            {
                column.Tag = PropertyOrder.Desc;
            }
            else
            {
                column.Tag = PropertyOrder.Asc;
            }

            return (PropertyOrder)column.Tag;
        }

        private void ClearTags(DataGridViewColumn except)
        {
            foreach (DataGridViewColumn column in Columns)
            {
                if (except != null && column.Equals(except)) continue;
                column.Tag = null;
                column.HeaderCell.SortGlyphDirection = SortOrder.None;
            }
        }
        
        public void InitProp(int columnIndex)
        {
            if (DataSource == null) return;

            var column = Columns[columnIndex];
            var t = GetItemType();

            string prop = column.DataPropertyName;

            if (string.IsNullOrEmpty(prop)) return;

            if (!propertyFormatCache.ContainsKey(prop))
            {
                var pi = t.GetProperty(prop);
                var a = AttributeHelper.Get<ColumnFormatAttribute>(pi);
                propertyFormatCache.Add(prop, a);
                propertyCache.Add(prop, pi);
            }
        }

        private Type GetItemType()
        {            
            Type listType = DataSource.GetType();
            var genArgs = listType.GetGenericArguments();

            if (genArgs.Length == 0) throw new InvalidOperationException("Invalid datasource type");
            return genArgs.FirstOrDefault();
        }




        private Dictionary<string, ColumnFormatAttribute> propertyFormatCache = new Dictionary<string, ColumnFormatAttribute>();
        private Dictionary<string, PropertyInfo> propertyCache = new Dictionary<string, PropertyInfo>();

        void DataGridViewEx_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            InitProp(e.ColumnIndex);

            var column = Columns[e.ColumnIndex];

            if (string.IsNullOrEmpty(column.DataPropertyName)) return;

            var item = propertyFormatCache[column.DataPropertyName];

            if (item == null) return;

            switch (item.ColumnFormat)
            {
                case ColumnFormat.Currency:
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                default:
                    break;
            }

            if (e.RowIndex < 0) return;

            if (e.Value != null)
            {
                switch (item.ColumnFormat)
                {
                    case ColumnFormat.Currency:
                        e.Value = FormatHelper.GetCurrency(e.Value.ToString());
                        break;

                    case ColumnFormat.Number:
                        e.Value = FormatHelper.GetNumber(e.Value.ToString());
                        break;
                    case ColumnFormat.Date:
                        DateTime dt = DateTime.Now;
                        DateTime.TryParse(e.Value.ToString(), out dt);
                        e.Value = dt;
                        break;

                    default:
                        break;
                }
            }

            //Rows[0].Cells[0].Style.WrapMode = DataGridViewTriState.True;
        }

        void DataGridViewEx_Resize(object sender, EventArgs e)
        {
            AutoSizeColumnsWidth();
            AutoSizeRowsHeight();
        }

        /*[DefaultValue(-1)]
        public int ImageColumnIndex { get; set; }

        void DataGridViewEx_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == ImageColumnIndex - 1 && e.Value != null)
            {
                //Columns[e.ColumnIndex].
                e.Value = new ImageResourceHandle(e.Value.ToString());
            }
        }*/

        public void AutoSizeColumnsWidth()
        {
            AutoResizeColumnHeadersHeight(false,true);

            if (DataSource == null)
                return;

            var args = (DataSource.GetType()).GetGenericArguments();

            if (args.Length == 0)
                return;

            Type t = args[0];
            //Type at = typeof(ColumnWidthAttribute);
            
            //DataGridViewColumn autoColumn = null;

            //foreach(var prop in t.GetProperties())
            //{
            //    var list = prop.GetCustomAttributes(at, true).OfType<ColumnWidthAttribute>();
            //    foreach (ColumnWidthAttribute a in list)
            //    {
            //        var column = GetColumn(prop.Name);
            //        if (column != null)
            //        {
            //            column.Width = a.Width;

            //            if (a.ColumnAutoSize == ColumnAutoSize.Fill)
            //            {
            //                column.MinimumWidth = 2;
            //                column.Width = 2;
            //                autoColumn = column;
            //            }
            //        }
            //    }
            //}

            //if (autoColumn != null)
            //{
            //    int totalWidth = 0;
            //    foreach (DataGridViewColumn col in Columns)
            //    {
            //        totalWidth += col.Width;
            //    }

            //    autoColumn.Width = totalWidth < Width ? Width - totalWidth - 20 : 100;
            //}

            Type at = typeof(ColumnInPercAttribute);

            int totalWidth = this.Width - 25;

            int fixedWidth = 0;
            foreach (var prop in t.GetProperties())
            {
                var list = prop.GetCustomAttributes(at, true).OfType<ColumnInPercAttribute>();
                foreach (ColumnInPercAttribute a in list.Where(x => x.Width > 0))
                {
                    var column = GetColumn(prop.Name);
                    if (column != null)
                    {
                        column.Width = a.Width;
                        fixedWidth += column.Width;
                    }
                }
            }

            foreach (var prop in t.GetProperties())
            {
                var list = prop.GetCustomAttributes(at, true).OfType<ColumnInPercAttribute>();
                foreach (ColumnInPercAttribute a in list.Where(x => x.Percentage > 0))
                {
                    var column = GetColumn(prop.Name);
                    if (column != null)
                    {
                        if (totalWidth - fixedWidth > 0)
                        {
                            column.Width = (totalWidth - fixedWidth) * a.Percentage / 100;
                        }
                        else column.Width = 100;
                        if (column.Width < 100) column.Width = 100; 
                    }
                }
            }

            AutoResizeColumnHeadersHeight();
        }

        public DataGridViewColumn GetColumn(string name)
        {
            return Columns.OfType<DataGridViewColumn>().Where(c => c.DataPropertyName == name).FirstOrDefault();
        }

        public T GetSelected<T>()
            where T:class
        {
            if (CurrentRow == null)
                return null;

            return CurrentRow.DataBoundItem as T;
        }

        public void SetSelected<T>(T value)
            where T:IEntity
        {
            IList list = DataSource as IList;
            var pe = list.OfType<T>().Where(p => Equals(p.Id, value.Id)).FirstOrDefault();
            int index = list.IndexOf(pe);

            if (index >= 0 && Rows.Count > index)
            {
                CurrentCell = Rows[index].Cells[0];
            }
            else
            {
                CurrentCell = null;
            }
        }

        public void SetSelectedByIndex(int index)
        {
            if (index >= 0 && Rows.Count > index)
            {
                CurrentCell = Rows[index].Cells[0];
            }
            else
            {
                CurrentCell = null;
            }
        }


        public TEntity GetSelected<TEntity, TPE>()
            where TEntity : Entity
            where TPE : PresentationEntity<TEntity>, new()
        {
            if (CurrentRow == null)
                return null;

            return (CurrentRow.DataBoundItem as TPE).Entity;
        }

        public List<TEntity> GetSelectedItems<TEntity, TPE>()
            where TEntity : Entity
            where TPE : PresentationEntity<TEntity>, new()
        {
            List<TEntity> lst = new List<TEntity>();
            if (!this.MultiSelect)
            {
                if (CurrentRow != null)
                    lst.Add((CurrentRow.DataBoundItem as TPE).Entity);
            }
            else
            {
                if (this.SelectedRows.Count > 0)
                {
                    foreach (DataGridViewRow r in this.SelectedRows)
                        lst.Add((r.DataBoundItem as TPE).Entity);
                }
            }
            return lst;
        }

        public TPE GetSelectedPE<TEntity, TPE>()
            where TEntity : Entity
            where TPE : PresentationEntity<TEntity>, new()
        {
            if (CurrentRow == null)
                return null;

            return CurrentRow.DataBoundItem as TPE;
        }

        public void SetSelected<TEntity, TPE>(TEntity value)
            where TEntity : Entity
            where TPE : PresentationEntity<TEntity>, new()
        {
            if (value == null)
                return;

            IList list = DataSource as IList;
            var pe = list.OfType<TPE>().Where(p => Equals(p.Entity.Id, value.Id)).FirstOrDefault();
            int index = list.IndexOf(pe);

            if (index >= 0 && Rows.Count > index)
            {
                CurrentCell = Rows[index].Cells[0];
            }
            else
            {
                CurrentCell = null;
            }
        }

        public void SetSelectedPE<TEntity, TPE>(TPE value)
            where TEntity : Entity
            where TPE : PresentationEntity<TEntity>, new()
        {
            IList list = DataSource as IList;
            var pe = list.OfType<TPE>().Where(p => p.Entity.Equals(value.Entity)).FirstOrDefault();
            int index = list.IndexOf(pe);

            if (index >= 0 && Rows.Count > index)
            {
                CurrentCell = Rows[index].Cells[0];
            }
            else
            {
                CurrentCell = null;
            }
        }

        public void AutoSizeRowsHeight()
        {
            if (Rows == null || Rows.Count == 0) return;
            
            for (int i = (CurrentPage * ItemsPerPage) - ItemsPerPage; i <= CurrentPage * ItemsPerPage; i++)
            {
                if (i < Rows.Count)
                    Rows[i].Height = Math.Max(Rows[i].GetPreferredHeight(i, DataGridViewAutoSizeRowMode.AllCellsExceptHeader, true), 23);
            }
            /*
            foreach (DataGridViewRow row in Rows)
            {
                row.Height = Math.Max(row.GetPreferredHeight(row.Index, DataGridViewAutoSizeRowMode.AllCellsExceptHeader, true), 23);
            }
            */
            
            Update();

            /*
            foreach (DataGridViewRow row in Rows)
            {
                //int maxHeight = 0;
                foreach (DataGridViewCell cell in row.Cells)
                {
                    string text = cell.FormattedValue != null ? cell.FormattedValue.ToString() : string.Empty;

                    var column = Columns[cell.ColumnIndex];

                    if (column is DataGridViewImageColumn) continue;

                    //maxHeight = Math.Max(maxHeight, GraphicHelper.GetHeight(text, Font, column.Width));
                }
                //row.Height = Math.Max(maxHeight, 23);
                row.Height = Math.Max(row.GetPreferredHeight(row.Index, DataGridViewAutoSizeRowMode.AllCells, true), 23);
            }
            Update();
            */
        }

        /// <summary>
        /// DataSource с сохранением пользовательской сортировки
        /// </summary>
        public object DataSourceEx
        {
            get { return DataSource; }
            set
            {
                if (EnableSort && DataSource != null)
                {
                    if (DataSource.GetType() == value.GetType())
                    {
                        foreach (DataGridViewColumn col in Columns)
                        {
                            if (col.Tag is PropertyOrder order)
                            {
                                Type itemType = GetItemType();
                                var propInfo = itemType.GetProperty(col.DataPropertyName);

                                var t = typeof(PropertyComparer<>).MakeGenericType(itemType);

                                var sortMethod = value.GetType().GetMethod("Sort", new[] { t });
                                var comparer = Activator.CreateInstance(t, new object[] { propInfo, order });

                                if (sortMethod == null)
                                {
                                    DataSource = value;
                                    return;
                                }


                                sortMethod.Invoke(value, new[] { comparer });


                                int ind = col.Index;

                                DataSource = value;
                                Columns[ind].Tag = order;

                                Columns[ind].HeaderCell.SortGlyphDirection = (order == PropertyOrder.Asc) ? SortOrder.Ascending : SortOrder.Descending;

                                return;
                            }
                        }
                    }
                }

                DataSource = value;
            }
        }
    }
}
