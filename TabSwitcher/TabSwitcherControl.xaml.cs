using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TabSwitcher
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class TabSwitcherControl : UserControl
    {
        public event RoutedEventHandler btnNextClick;
        public event RoutedEventHandler btnPreviousClick;

        private bool bHidebtnPrevious = false; // поле, соответствующее тому, будет ли скрыта кнопка Назад
        private bool bHidebtnNext = false; // поле, соответствующее тому, будет ли скрыта кнопка Вперед

        public TabSwitcherControl()
        {
            InitializeComponent();
        }

        public bool IsHidebtnPrevious
        {
            get
            {
                return bHidebtnPrevious;
            }
            set
            {
                bHidebtnPrevious = value;
                SetButtons(); // метод, который отвечает за отрисовку кнопок
            }
        }

        public bool IsHidebtnNext
        {
            get
            {
                return bHidebtnNext;
            }
            set
            {
                bHidebtnNext = value;
                SetButtons(); // метод, который отвечает за отрисовку кнопок
            }
        }

        private void BtnNextTruebtnPreviousFalse()
        {
            btnNext.Visibility = Visibility.Hidden;
            btnPrevious.Visibility = Visibility.Visible;
            btnPrevious.Width = 224;
            btnNext.Width = 0;
            btnPrevious.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        private void btnPreviousTrueBtnNextFalse()
        {
            btnPrevious.Visibility = Visibility.Hidden;
            btnNext.Visibility = Visibility.Visible;
            btnNext.Width = 224;
            btnPrevious.Width = 0;
            btnNext.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        private void btnPreviousFalseBtnNextFalse()
        {
            btnNext.Visibility = Visibility.Visible;
            btnPrevious.Visibility = Visibility.Visible;
            btnNext.Width = 112;
            btnPrevious.Width = 112;
        }

        private void btnPreviousTrueBtnNextTrue()
        {
            btnPrevious.Visibility = Visibility.Hidden;
            btnNext.Visibility = Visibility.Hidden;
        }

        private void SetButtons()
        {
            if (bHidebtnPrevious && bHidebtnNext) btnPreviousTrueBtnNextTrue();
            else if (!bHidebtnNext && !bHidebtnPrevious) btnPreviousFalseBtnNextFalse();
            else if (bHidebtnNext && !bHidebtnPrevious) BtnNextTruebtnPreviousFalse();
            else if (!bHidebtnNext && bHidebtnPrevious) btnPreviousTrueBtnNextFalse();
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            btnPreviousClick?.Invoke(sender, e);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            btnNextClick?.Invoke(sender, e);
        }
    }
}
