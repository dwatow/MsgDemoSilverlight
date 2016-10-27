using KgsSilverlightCommon;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
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

                foreach (var element in cv_MemberValue)
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
}
