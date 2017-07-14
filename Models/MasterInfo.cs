using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZeroStartAPI.Models
{
    public class MasterInfo
    {
    }
    
    public class ViewMastInfoModel : masterinfo
    {
        public string strServices { get; set; }
        public int iReadCount { get; set; }
        public int iServiceCount { get; set; }
        public int iRete { get; set; }
        public int iBuyed { get; set; }
        public List<ViewmroominfoModel> lmroominfo;

    }
    public class ViewFileInfo
    {
        public string fileurl { get; set; }
        public string filetype { get; set; }
    }
    public class ViewmroominfoModel : mroominfo
    {
        public List<ViewmrcrinfoModel> ltmrcrinfos;
        public List<ViewFileInfo> ltFilesVideo;
        public List<ViewFileInfo> ltFilesImg;
        public List<ViewFileInfo> ltFilesAudio;

    }
    public class ViewmrcrinfoModel : mrcrinfo
    {
        public int IsMe { get; set; }
        public string susername { get; set; }
        public string rusername { get; set; }
    }
    
    public class ViewRoomInfoModel
    {
        public int iOwner { get; set; }
        public List<ViewmroominfoModel> ltRoomInfos;
        public string username { get; set; }
        public string userpic { get; set; }
    }
    public class ViewSRoomInfoModel : sroominfo
    {

    }
    public class  ViewSrcontenInfoModel: srcontent
    {
        public int iHaveRead { get; set; }
    }
    public class ViewMessageinfoModel : cmessageinfo
    {
        public int iHaveRead { get; set; }
        // public int iHaveRead { get; set; }
    }
    public class getAmount
    {
        public string applytime { get; set; }
        public string applyamount { get; set; }
        public int applyed { get; set; }

    }
    public class ViewWallet
    {
        //钱包金额、可提现金额、提现列表（提现申请时间、提现金额、提现状态）
        public int  curAmount { get; set; }
        public int applyAmount { get; set; }
        public List<getAmount> getAmount;
        // public int iHaveRead { get; set; }
    }
}