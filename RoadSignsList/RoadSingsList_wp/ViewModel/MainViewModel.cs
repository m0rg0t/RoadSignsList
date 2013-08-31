using GalaSoft.MvvmLight;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace RoadSingsList_wp.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }

        public async Task<string> MakeWebRequest(string url = "")
        {
            HttpClient http = new System.Net.Http.HttpClient();
            HttpResponseMessage response = await http.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        public string TruncateLongString(string str, int maxLength)
        {
            if (str.Length > maxLength)
            {
                return str.Substring(0, Math.Min(str.Length, maxLength)) + "...";
            }
            else
            {
                return str.Substring(0, Math.Min(str.Length, maxLength));
            }
        }

        private ObservableCollection<SignItem> _items = new ObservableCollection<SignItem>();
        /// <summary>
        /// Вcе цены
        /// </summary>
        public ObservableCollection<SignItem> Items
        {
            get { return _items; }
            set { 
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        private ObservableCollection<SignItem> _resultItems = new ObservableCollection<SignItem>();
        /// <summary>
        /// Результаты поиска
        /// </summary>
        public ObservableCollection<SignItem> ResultItems
        {
            get { return _resultItems; }
            set
            {
                _resultItems = value;
                RaisePropertyChanged("ResultItems");
            }
        }

        private string _searchQuery;

        public string SearchQuery
        {
            get { return _searchQuery; }
            set { 
                _searchQuery = value;

                var items = from item in Items
                            where item.Title.ToLower().Contains(_searchQuery.ToLower()) ||
                                item.Description.ToLower().Contains(_searchQuery.ToLower())
                            select item;
                ResultItems = new ObservableCollection<SignItem>();
                foreach (var item in items)
                {
                    ResultItems.Add(item);
                };
                RaisePropertyChanged("ResultItems");
                RaisePropertyChanged("SearchQuery");
            }
        }
        

        public async Task<bool> LoadData()
        {
            Loading = true;
            var responseText = await MakeWebRequest("http://api.pub.emp.msk.ru:8081/json/v10.0/transport/pdd/getallsigns?token=" + App.TOKEN + ""); //" + App.TOKEN);
            try
            {
                JObject o = JObject.Parse(responseText.ToString());
                //SampleDataSource._sampleDataSource = new ObservableCollection<SampleDataGroup>();

                int count = 1;
                foreach (var item in o["result"])
                {
                    string shorttitle = item["name"].ToString().Trim();
                    //shorttitle = TruncateLongString(shorttitle, 36);

                    foreach (var ticket in item["objects"])
                    {
                        try
                        {
                            SignItem price = new SignItem();
                            price.ShortGroupName = shorttitle;
                            price.GroupName = item["name"].ToString();
                            price.Title = ticket["part_name"].ToString();
                            price.Description = item["name"].ToString();
                            price.Image = ticket["signs_src"].ToString();

                            Items.Add(price);
                        }
                        catch { };
                    };
                }
            }
            catch
            {
                Loading = false;
            };
            RaisePropertyChanged("Items");
            Loading = false;
            return true;
        }

        private bool _loading;

        public bool Loading
        {
            get { return _loading; }
            set { 
                _loading = value;
                RaisePropertyChanged("Loading");
            }
        }


        


    }
}