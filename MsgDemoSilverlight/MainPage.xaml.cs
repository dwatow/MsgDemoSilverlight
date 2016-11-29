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
    public partial class MainPage : UserControl
    {
        private KFabLinkWebEventClient cv_WebEventClient = new KFabLinkWebEventClient();

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
            ReplyBody.Text = MessageId.Text + "傳送中..." + Environment.NewLine + ReplyBody.Text;
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
            FunctionSelect.SelectedTabIndex = 2;
        }

        private void DiffMessage_Click(object sender, RoutedEventArgs e)
        {
            KXmlItemCheckReport left = new KXmlItemCheckReport(new KXmlItem(LeftXml.Text));
            left.Diff(new KXmlItem(RightXml.Text));

            if (!left.CheckIsOK)
            {
                LeftXml.Text = left.strXml;
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
            FunctionSelect.SelectedTabIndex = 2;
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

        private void FiddleReplyCode_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            try
            {
                string strFiddleReplyCode = FiddleReplyCode.Text;
                if (!string.IsNullOrWhiteSpace(strFiddleReplyCode))
                {
                    byte[] buffer = SysUtils.HexToBinary(strFiddleReplyCode);
                    byte[] unzip_data = SysUtils.UnZip(buffer);
                    KXmlItem xml_buffer = SysUtils.LoadXmlItemFromMemoryBuffer(unzip_data);
                    DecodeToXml.Text = xml_buffer.Text;
                }
            }
            catch(Ionic.Zlib.ZlibException zipe)
            {
                string str = zipe.ToString();  //消除warning
                DecodeToXml.Text = @"無法解碼的值";
            }
        }

        string cv_TempFiddleReplyCodeForGotFocus;

        private void FiddleReplyCode_GotFocus(object sender, RoutedEventArgs e)
        {
            cv_TempFiddleReplyCodeForGotFocus = FiddleReplyCode.Text;
            FiddleReplyCode.Clear();
        }

        private void FiddleReplyCode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FiddleReplyCode.Text))
            {
                FiddleReplyCode.Text = cv_TempFiddleReplyCodeForGotFocus;
            }
        }

        public MainPage()
        {
            InitializeComponent();
            cv_WebEventClient.OnSecondaryReceived += OnFabLinkClientSecondaryReceived;
            TargetModule.Text = @"DM";
            ChannelId.Text = @"DMChannel1";
            MessageId.Text = @"EAI_ReleaseAllObjsRequest";
            RequestBody.Text = @"<Body></Body>";
        }
    }
}
