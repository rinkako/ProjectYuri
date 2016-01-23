﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LyyneheymCore.SlyviaPile
{
    /// <summary>
    /// 语法节点类：构成语法树的最小单元
    /// </summary>
    public sealed class SyntaxTreeNode
    {
        // 构造函数
        public SyntaxTreeNode(SyntaxType type = SyntaxType.Unknown, SyntaxTreeNode parent = null)
        {
            this.nodeName = type.ToString();
            this.nodeSyntaxType = type;
            this.parent = parent;
        }
        // 绑定处理函数
        internal CandidateFunction candidateFunction = null;
        // 子树向量
        public List<SyntaxTreeNode> children = null;
        // 父指针
        public SyntaxTreeNode parent = null;
        // 命中token附加值
        public string nodeValue = null;
        // 命中产生式类型
        public CFunctionType nodeType = CFunctionType.None;
        // 节点名字
        public string nodeName = "";
        // 附加值
        public object aTag = null;
        // 错误位
        public bool errorBit = false;
        // 不推导节点参数孩子字典
        public Dictionary<string, SyntaxTreeNode> paramDict = null;
        // 不推导节点参数Token子流
        internal List<Token> paramTokenStream = null;
        // 节点变量的引用
        public object nodeVarRef = null;
        // 命中语法结构类型
        private SyntaxType nodeSyntaxTyper = SyntaxType.Unknown;
        // 命中语法结构类型
        public SyntaxType nodeSyntaxType
        {
            get
            {
                return nodeSyntaxTyper;
            }
            set
            {
                nodeSyntaxTyper = value;
                this.nodeName = value.ToString();
            }
        }

        /// <summary>
        /// 树的递归遍历文本化
        /// </summary>
        /// <returns>表示树的字符串</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("> SyntaxTreeNode String Format: ");
            builder.AppendLine(this.nodeName + ", Type:" + this.nodeSyntaxType.ToString() + ", Func:" + this.nodeType.ToString() + "");
            int identation = 0;
            this.GetTree(builder, this, ref identation, false);
            return builder.ToString();
        }

        /// <summary>
        /// 递归遍历显示
        /// </summary>
        /// <param name="builder">字符串构造器</param>
        /// <param name="myNode">当前节点</param>
        /// <param name="identation">缩进量</param>
        private void GetTree(StringBuilder builder, SyntaxTreeNode myNode, ref int identation, bool dflag)
        {
            // 如果空就没必要继续了
            if (myNode == null)
            {
                return;
            }
            // 画树
            builder.Append(DrawTree(myNode));
            if (dflag)
            {
                builder.Append("[d]");
            }
            builder.Append(myNode.nodeName.ToString());
            if (myNode.nodeSyntaxType >= SyntaxType.Unknown
              && myNode.nodeSyntaxType != SyntaxType.epsilonLeave
              && myNode.nodeSyntaxType != SyntaxType.tail_startEndLeave)
            {
                builder.Append(" (" + myNode.nodeValue + ")");
            }
            else if (myNode.nodeSyntaxType == SyntaxType.synr_dialog)
            {
                string sub = myNode.nodeValue.Replace("\r", "").Replace("\n", "");
                builder.Append(" (" + (sub.Length < 12 ? sub : sub.Substring(0, 11) + " ...") + ")");
            }
            builder.Append(Environment.NewLine);
            // 缩进并打印结果
            identation++;
            if (myNode.nodeSyntaxType.ToString().StartsWith("synr_") && myNode.paramDict != null)
            {
                foreach (KeyValuePair<string, SyntaxTreeNode> kvp in myNode.paramDict)
                {
                    GetTree(builder, kvp.Value, ref identation, true);
                }
            }
            if (myNode.children != null)
            {
                for (int i = 0; i < myNode.children.Count; i++)
                {
                    GetTree(builder, myNode.children[i], ref identation, false);
                }
            }
            // 回归缩进
            identation--;
        }

        /// <summary>
        /// 获取缩进
        /// </summary>
        /// <param name="myNode">当前节点</param>
        /// <returns>树的缩进字符串</returns>
        private string DrawTree(SyntaxTreeNode myNode)
        {
            // 若空就不需要继续了
            if (myNode == null)
            {
                return "";
            }
            // 取父母节点，若空就不需要画线了
            SyntaxTreeNode parent = myNode.parent;
            if (parent == null)
            {
                return "";
            }
            // 否则查询祖父母节点来看父母节点的排位
            List<bool> lstline = new List<bool>();
            while (parent != null)
            {
                SyntaxTreeNode pp = parent.parent;
                int indexOfParent = 0;
                if (pp != null)
                {
                    if (pp.nodeSyntaxType.ToString().StartsWith("synr_") && pp.paramDict != null)
                    {
                        foreach (KeyValuePair<string, SyntaxTreeNode> kvp in pp.paramDict)
                        {
                           if (kvp.Value == parent)
                           {
                               break;
                           }
                           else
                           {
                               indexOfParent++;
                           }
                        }
                        int nocCount = 0;
                        if (pp.children != null)
                        {
                            nocCount += pp.children.Count;
                        }
                        lstline.Add(indexOfParent < pp.paramDict.Count + nocCount - 1);
                    }
                    else if (pp.children != null)
                    {
                        for (; indexOfParent < pp.children.Count; indexOfParent++)
                        {
                            if (parent == pp.children[indexOfParent])
                            {
                                break;
                            }
                        }
                        lstline.Add(indexOfParent < pp.children.Count - 1);
                    }
                }
                parent = pp;
            }
            // 画纵向线
            string builder = "";
            for (int i = lstline.Count - 1; i >= 0; i--)
            {
                builder += lstline[i] ? "│  " : "    ";
            }
            // 获得自己在兄弟姐妹中的排行
            parent = myNode.parent;
            int indexOfParent2 = 0;
            if (parent.nodeSyntaxType.ToString().StartsWith("synr_") && parent.paramDict != null)
            {
                //foreach (KeyValuePair<string, SyntaxTreeNode> kvp in parent.paramDict)
                //{
                //    if (kvp.Value == parent)
                //    {
                //        break;
                //    }
                //    else
                //    {
                //        indexOfParent2++;
                //    }
                //}
                //// 如果是最后一个就不要出头了
                //if (indexOfParent2 < parent.paramDict.Count - 1)
                //{
                //    builder += "├─";
                //}
                //else
                //{
                //    builder += "└─";
                //}
                builder += "└─";
            }
            else if (parent.children != null)
            {
                for (; indexOfParent2 < parent.children.Count; indexOfParent2++)
                {
                    if (myNode == parent.children[indexOfParent2])
                    {
                        break;
                    }
                }
                // 如果是最后一个就不要出头了
                if (indexOfParent2 < parent.children.Count - 1)
                {
                    builder += "├─";
                }
                else
                {
                    builder += "└─";
                }
            }
            return builder;
        }
    }

    /// <summary>
    /// 枚举：语法节点类型
    /// </summary>
    public enum SyntaxType
    {
        // 段落
        synr_dialog,
        // 段落结束符
        synr_dialogTerminator,
        // 显示文本
        synr_a,
        // 显示图片
        synr_picture,
        // 移动图片
        synr_move,
        // 消去图片
        synr_deletepicture,
        // 显示立绘
        synr_cstand,
        // 消去立绘
        synr_deletecstand,
        // 播放声效
        synr_se,
        // 播放音乐
        synr_bgm,
        // 停止音乐
        synr_stopbgm,
        // 播放语音
        synr_vocal,
        // 停止语音
        synr_stopvocal,
        // 返回标题
        synr_title,
        // 调用菜单
        synr_menu,
        // 调用存档
        synr_save,
        // 调用读档
        synr_load,
        // 标签
        synr_lable,
        // 标签跳转
        synr_jump,
        // 函数调用
        synr_call,
        // 循环（头）
        synr_for,
        // 循环（尾）
        synr_endfor,
        // 条件（头）
        synr_if,
        // 条件（分支）
        synr_else,
        // 条件（尾）
        synr_endif,
        // 函数声明
        synr_function,
        // 函数结束
        synr_endfunction,
        // 剧本跳转
        synr_scene,
        // 开关操作
        synr_switch,
        // 变量操作
        synr_var,
        // 退出循环
        synr_break,
        // 退出程序
        synr_shutdown,
        // 等待
        synr_wait,
        // 选择支
        synr_branch,
        // 参数：函数签名
        para_sign,
        // 参数：选择支链
        para_link,
        // 参数：名称
        para_name,
        // 参数：语音id
        para_vid,
        // 参数：立绘表情
        para_face,
        // 参数：序号
        para_id,
        // 参数：x坐标
        para_x,
        // 参数：y坐标
        para_y,
        // 参数：z坐标
        para_z,
        // 参数：x加速度
        para_xacc,
        // 参数：y加速度
        para_yacc,
        // 参数：透明度
        para_capacity,
        // 参数：x轴缩放比
        para_xscale,
        // 参数：y轴缩放比
        para_yscale,
        // 参数：时间
        para_time,
        // 参数：文件名
        para_filename,
        // 参数：音轨号
        para_track,
        // 参数：条件子句
        para_cond,
        // 参数：表达式
        para_dash,
        // 参数：位置
        para_loc,
        // 参数：角度
        para_ro,
        // 参数：音量
        para_vol,
        // 参数：开光状态
        para_state,
        // 参数值：左边
        para_left,
        // 参数值：中间
        para_mid,
        // 参数值：右边
        para_right,
        // 根节点
        case_kotori,
        // <disjunct> ::= <conjunct> <disjunct_pi>;
        case_disjunct,
        // <disjunct_pi> ::= "||" <conjunct> <disjunct_pi> | null;
        case_disjunct_pi,
        // <conjunct> ::= <bool> <conjunct_pi>;
        case_conjunct,
        // <conjunct_pi> ::= "&&" <bool> <conjunct_pi> | null;
        case_conjunct_pi,
        // <bool> ::= "(" <disjunct> ")" | "!" <bool> | <comp>;
        case_bool,
        // <comp> ::= <wexpr> <rop> <wexpr>;
        case_comp,
        // <rop> ::= "<>" | "==" | ">" | "<" | ">=" | "<=" | null;
        case_rop,
        // <wexpr> ::= <wmulti> <wexpr_pi>;
        case_wexpr,
        // <wexpr> ::= <wplus> <wexpr_pi> | null;
        case_wexpr_pi,
        // <wplus> ::= "+" <wmulti> | "-" <wmulti>;
        case_wplus,
        // <wmulti> ::= <wunit> <wmultiOpt>;
        case_wmulti,
        // <wmultiOpt> ::= "*" <wunit> | "/" <wunit> | null;
        case_wmultiOpt,
        // <wunit> ::= number | identifier | "-" <wunit> | "+" <wunit> | "(" <wexpr> ")";
        case_wunit,
        // 未知的语法结点符号
        Unknown,
        // identifier
        tail_idenLeave,
        // "("
        tail_leftParentheses_Leave,
        // ")"
        tail_rightParentheses_Leave,
        // ";"
        tail_semicolon_Leave,
        // ","
        tail_comma_Leave,
        // null
        epsilonLeave,
        // "="
        tail_equality_Leave,
        // "+"
        tail_plus_Leave,
        // "-"
        tail_minus_Leave,
        // "*"
        tail_multiply_Leave,
        // "/"
        tail_divide_Leave,
        // number
        numberLeave,
        // cluster
        clusterLeave,
        // "||"
        tail_or_Or_Leave,
        // "&&"
        tail_and_And_Leave,
        // "!"
        tail_not_Leave,
        // "<>"
        tail_lessThan_GreaterThan_Leave,
        // "=="
        tail_equality_Equality_Leave,
        // ">"
        tail_greaterThan_Leave,
        // "<"
        tail_lessThan_Leave,
        // ">="
        tail_greaterThan_Equality_Leave,
        // "<="
        tail_lessThan_Equality_Leave,
        // #
        tail_startEndLeave
    }
}
