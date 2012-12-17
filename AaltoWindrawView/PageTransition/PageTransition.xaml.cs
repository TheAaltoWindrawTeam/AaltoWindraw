using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace AaltoWindraw
{
    public partial class PageTransition : UserControl
    {
        #region Properties
        Stack<UserControl> pageStack = new Stack<UserControl>();
        
        public static readonly DependencyProperty TransitionTypeProperty = DependencyProperty.Register("TransitionType",
            typeof(PageTransitionType),
            typeof(PageTransition), new PropertyMetadata(PageTransitionType.SlideAndFade));

        public PageTransitionType TransitionType
        {
            get
            {
                return (PageTransitionType)GetValue(TransitionTypeProperty);
            }
            set
            {
                SetValue(TransitionTypeProperty, value);
            }
        }
        #endregion

        public PageTransition()
        {
            InitializeComponent();
        }

        public void DisplayPage()
        {
            DoDisplayPage(pageStack.Peek());
        }

        public void DoDisplayPage(UserControl page)
        {
            contentPresenter.Content = page;
        }

        public void NextPage(UserControl page)
        {
            if (pageStack.Any())
            {
                ((BasicPage)page).setPossibleGoBack(true);
            }
            pageStack.Push(page);

            //Storyboard hidePage = (Resources[string.Format("{0}Out", TransitionType.ToString())] as Storyboard).Clone();
            //hidePage.Begin(contentPresenter);

            DisplayPage();
        }

        public void PreviousPage()
        {
            pageStack.Pop();
            DisplayPage();
        }

        public void EmptyPageStack()
        {
            pageStack.Clear();
        }

    }
}