using UnityEngine;
using TMPro;

namespace LiveAppUI.View
{
    public class ObservableLabelTMPro : ObservableLabel
    {
        [SerializeField] private TMP_Text m_Text = null;

        public override string Text
        {
            get { return m_Text.text; }
            set { m_Text.text = value; }
        }

        /// <summary>
        /// TextMeshProのSetCharArrayを使用してテキストを反映する
        /// TextMeshProのSetCharArrayはRunTimeではGCAllocしないため、毎フレーム実行するものにはこちらを推奨
        /// </summary>
        /// <param name="charArray">Labelに表示するテキストをchar[]で指定する</param>
        public void SetCharArray( char[] charArray )
        {
            m_Text.SetCharArray( charArray );
        }

        public override void SetAlignment( TextAlignment alignment )
        {
            switch( alignment )
            {
                case TextAlignment.Center:
                    m_Text.alignment = TextAlignmentOptions.Center;
                    return;
                case TextAlignment.Left:
                    m_Text.alignment = TextAlignmentOptions.MidlineLeft;
                    return;
                case TextAlignment.Right:
                    m_Text.alignment = TextAlignmentOptions.Right;
                    return;
            }
        }
    }
}
