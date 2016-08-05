using KgsSilverlightCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;



namespace MsgDemoSilverlight
{
    public class KXmlItemCheckReport
    {
        private int cv_Level;
        private string cv_Name;
        private KXmlItemType cv_Type;
        private int cv_Number;
        private string cv_Change;
        private List<KXmlItemCheckReport> cv_MemberValue = new List<KXmlItemCheckReport>();
        public KXmlItemCheckReport(KXmlItem m_OriginalKXmlItem, int m_Level = 0)
        {
            cv_Level = m_Level;
            cv_Name = m_OriginalKXmlItem.Name;
            cv_Type = m_OriginalKXmlItem.ItemType;
            cv_Number = m_OriginalKXmlItem.ItemNumber;
            if (cv_Type == KXmlItemType.itxList)
            {
                int level = cv_Level + 1;
                int size_xml = m_OriginalKXmlItem.ItemNumber;
                for (int i = 0; i < size_xml; ++i)
                {
                    KXmlItemCheckReport element = new KXmlItemCheckReport(m_OriginalKXmlItem.Items[i], level);
                    cv_MemberValue.Add(element);
                }
                strXml = @"<" + cv_Name + @" KGS_TYPE=""" + cv_Type + @""" KGS_ITEM_NUMBER=""" + cv_Number + @""">";
                cv_EndXml = @"</" + cv_Name + ">";
            }
            else
            {
                strXml = m_OriginalKXmlItem.Text;
            }
        }

        public void Diff(KXmlItem m_ChangeKXmlItem)
        {
            if (string.IsNullOrWhiteSpace(m_ChangeKXmlItem.Name))
                return;

            if (cv_Name != m_ChangeKXmlItem.Name)
            {
                CheckIsOK = false;
                if (string.IsNullOrWhiteSpace(cv_Change))
                    cv_Change += " Name:" + m_ChangeKXmlItem.Name;
            }

            if (cv_Type != m_ChangeKXmlItem.ItemType)
            {
                CheckIsOK = false;
                if (string.IsNullOrWhiteSpace(cv_Change))
                    cv_Change += " ItemType:" + m_ChangeKXmlItem.ItemType;
            }
            
            if (cv_Type == KXmlItemType.itxList)
            {
                DiffMutiVar(m_ChangeKXmlItem);
            }
            else
            {
                DiffSignleVar(m_ChangeKXmlItem);
            }
        }

        private void DiffSignleVar(KXmlItem m_ChangeKXmlItem)
        {
            if (cv_Xml != m_ChangeKXmlItem.Text)
            {
                CheckIsOK = false;
                if (string.IsNullOrWhiteSpace(cv_Change))
                    cv_Change += " Text:" + m_ChangeKXmlItem.Text;
            }
            else
            {
                CheckIsOK = true;
            }
        }

        private void DiffMutiVar(KXmlItem m_ChangeKXmlItem)
        {
            CheckIsOK = true;
            if (IsObject())
            {
                //Obj內容可多不可少
                foreach (var element in cv_MemberValue)
                {
                    if (element.CheckIsOK == true)
                    {
                        continue;
                    }

                    string member_value_name = element.cv_Name;
                    KXmlItem xml_member_value = m_ChangeKXmlItem.ItemsByLevelName[member_value_name];
                    element.Diff(xml_member_value);
                }
            }
            else
            {
                //List的數量必須相同

                if (cv_Number != m_ChangeKXmlItem.ItemNumber)
                {
                    CheckIsOK = false;
                    if (string.IsNullOrWhiteSpace(cv_Change))
                        cv_Change += " ItemNumber:" + m_ChangeKXmlItem.ItemNumber;
                }

                int size_xml = m_ChangeKXmlItem.ItemNumber;
                for (int i = 0; i < size_xml; ++i)
                {
                    foreach (var orig_element in cv_MemberValue)
                    {
                        if (orig_element.CheckIsOK == true)
                        {
                            continue;
                        }

                        KXmlItem xml_change_element = m_ChangeKXmlItem.Items[i];
                        orig_element.Diff(xml_change_element);
                    }
                }
            }

            foreach (var element in cv_MemberValue)
            {
                if (element.CheckIsOK == false)
                {
                    CheckIsOK = false;
                }
            }
        }

        private bool IsObject()
        {
            if (cv_MemberValue.Count >= 2)
            {
                string name0 = cv_MemberValue[0].cv_Name;
                string name1 = cv_MemberValue[1].cv_Name;
                if (name1 == name0)
                {
                    //有兩個元素, 名稱相同 -> 容器
                    return false; //is list
                }
            }

            //有兩個元素, 名稱不同 -> 物件
            //只有一個元素 -> 物件
            return true;
        }

        private bool cv_CheckIsOK;

        public bool CheckIsOK
        {
            get { return cv_CheckIsOK; }
            set { cv_CheckIsOK = value; }
        }

        private string cv_EndXml;
        private string cv_Xml;

        public string strXml
        {
            get 
            {
                string xml;
                if (CheckIsOK == true)
                {
                    xml = cv_Xml;
                }
                else if (CheckIsOK == false)
                {
                    xml = cv_Xml + " --- x, " + cv_Change;
                }
                else
                {
                    xml = cv_Xml + " --- ?";
                }
                xml += Environment.NewLine;

                foreach(var element in cv_MemberValue)
                {
                    xml += element.strXml;
                }

                //排版
                string str_level = "";
                for (int i = 0; i < cv_Level; ++i)
                {
                    str_level = str_level + "    ";
                }

                if (cv_Type == KXmlItemType.itxList)
                {
                    xml += str_level + cv_EndXml + Environment.NewLine;
                }

                return str_level + xml;
            }
            set 
            { 
                cv_Xml = value; 
            }
        }

        public KXmlItem GetKXmlItem()
        {
            KXmlItem xml = new KXmlItem();
            xml.Text = strXml;
            return xml;
        }
    }

    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            cv_WebEventClient.OnSecondaryReceived += OnFabLinkClientSecondaryReceived;
            TargetModule.Text = @"DM";
            ChannelId.Text = @"DMChannel1";
            MessageId.Text = @"WIP_QueryWorkingReportDeviceConfigRequest";
            RequestBody.Text = @"<Body KGS_TYPE=""L"" KGS_ITEM_NUMBER=""5"">
    <RequireTarget KGS_TYPE=""U4"" KGS_ITEM_NUMBER=""1"">2</RequireTarget>
    <WorkOrder KGS_TYPE=""A"" KGS_ITEM_NUMBER=""23"">TEST_CONFIG_S1IN18GROUP</WorkOrder>
    <LotNo KGS_TYPE=""A"" KGS_ITEM_NUMBER=""0""/>
    <SerialNo KGS_TYPE=""A"" KGS_ITEM_NUMBER=""0""/>
    <DeviceId KGS_TYPE=""A"" KGS_ITEM_NUMBER=""0""/>
</Body>";
        }

        KFabLinkWebEventClient cv_WebEventClient = new KFabLinkWebEventClient();


        private void SendRequest(string m_MessageId, string m_Source, string m_Targe, string m_ChannelId, KXmlItem xml)
        {
            KFabLinkMessage message = new KFabLinkMessage();
            message.Name = m_MessageId;
            message.Source = m_Source;
            message.Target = m_Targe;
            message.MessageType = KFabLinkMessageType.fmtRequest;
            message.Data = xml;

            cv_WebEventClient.ModuleId = m_Source;
            cv_WebEventClient.SendPrimary(m_ChannelId, m_MessageId, m_Targe, message, 10000);
        }
        private void OnFabLinkClientSecondaryReceived(object m_Sender, string m_ChannelId, string m_MessageId, KFabLinkTransaction m_Transaction, KFabLinkMessage m_Message)
        {
            ReplyBody.Text = m_Message.Data.Text;
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            ReplyBody.Text = ReplyBody.Text + "\n" + MessageId.Text + "傳送中...";
            KXmlItem xml = new KXmlItem();
            xml.Name = @"Body";
            xml.Text = RequestBody.Text;

            SendRequest(MessageId.Text, "ISAPI_CLIENT", TargetModule.Text, ChannelId.Text, xml);
        }

        private void ToDiffLeft_Click(object sender, RoutedEventArgs e)
        {
            LeftXml.Text = ReplyBody.Text;
            GoToDiffLeftButton.IsEnabled = false;
        }
        private void ToDiffRight_Click(object sender, RoutedEventArgs e)
        {
            RightXml.Text = ReplyBody.Text;
            FunctionSelect.SelectedTabIndex = 1;
        }

        private void DiffMessage_Click(object sender, RoutedEventArgs e)
        {
            KXmlItem left = new KXmlItem();
            left.Text = LeftXml.Text;
            KXmlItemCheckReport original_kxmlitem = new KXmlItemCheckReport(left);

            KXmlItem right = new KXmlItem();
            right.Text = RightXml.Text;
            original_kxmlitem.Diff(right);
            if (original_kxmlitem.CheckIsOK != true)
            {
                LeftXml.Text = original_kxmlitem.strXml;
            }
            else
            {
                MessageBox.Show("沒有任何錯誤...");
            }
        }

        private void FunctionSelect_SelectedTabChildChanged(object sender, DevExpress.Xpf.Core.ValueChangedEventArgs<FrameworkElement> e)
        {
            GoToDiffLeftButton.IsEnabled = true;
            GoToDiffLeftButton.IsEnabled = true;
        }

        private void LeftXml_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            int curr_select_index = FunctionSelect.SelectedTabIndex;
            FunctionSelect.SelectedTabIndex = 1;
            if (LeftXml.Text.Length != 0 && RightXml.Text.Length != 0)
            {
                DiffMsgButton.IsEnabled = true;
            }
            else
            {
                DiffMsgButton.IsEnabled = false;
            }
            FunctionSelect.SelectedTabIndex = curr_select_index;
        }   
    }
}
