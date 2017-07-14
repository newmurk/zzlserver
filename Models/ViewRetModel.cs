using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ZeroStartAPI.Models
{
    public class ViewRetModel
    {

    }
    public class DataCatalogs
    {
        public int catalogid { get; set; }
        public string catainfo { get; set; }
        public int sid { get; set; }
    }
    public class KeyCount
    {
        public int Key { get; set; }
        public int Total { get; set; }
    }
    public class KeyCountSharp
    {
        public string Key { get; set; }
        public int Total { get; set; }
    }
    public class ViewDataInfoModel: datainfo
    {
        public int Buyed { get; set; }
        public int BuyedCount { get; set; }
        public string IconFlag { get; set; }
        public IEnumerable<string> UserPicUrl { get; set; }
        public IEnumerable<DataCatalogs> DataCatalogArry { get; set; }

    }
    public class ViewRankListMoreModel : rankinglist
    {
        public string NikeName { get; set; }
        public string UserPic { get; set; }

    }
    public class rankingInfoList
    {
        public string NickName { get; set; }
        public string PicAddress { get; set; }
        public int Position { get; set; }
        public int SharpCount { get; set; }
        public int IFlag { get; set; }
    }
    public class ViewRankListModel : rankinglist
    {
        public List<rankingInfoList> ltRankingInfo { get; set; }
    }


    
    public class ViewPayInfoModel : payinfo
    {
        public string dataInfo { get; set; }
        public string url { get; set; }
        public string signature { get; set; }
        public string picurs { get; set; }
        public string picurd { get; set; }
        //public IEnumerable<DataCatalogs> DataCatalogArry { get; set; }

    }
    public class ViewOgQuestionnaireModel
    {
        public string name { get; set; }
        public int Buyed { get; set; }
        public int qid { get; set; }

    }
    public class ImgMem
    {
        public int succeed { get; set; }
        public MemoryStream picm { get; set; }
        public string roomkey { get; set; }
        public int roomid { get; set; }
    }
}