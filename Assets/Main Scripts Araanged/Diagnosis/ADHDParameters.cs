using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

[System.Serializable]
public class InattentionMetric
{
    [Tooltip("부주의 척도")] 
    
    [ReadOnly,SerializeField] 
    private string _dSM_Five;

    [SerializeField]
    public string _vR_observable_actions;
    
    [ReadOnly, SerializeField] 
    public int value; 

    public InattentionMetric(string _dSM_Five, string _vR_observable_actions, int initialValue = 0)
    {
        this._dSM_Five = _dSM_Five;
        this._vR_observable_actions = _vR_observable_actions;
        this.value = initialValue;
    }
    public string DSM_Five => _dSM_Five;
}

[System.Serializable]
public class HyperActivityMetric
{
    [Tooltip("과잉행동 척도")] 
    [ReadOnly,SerializeField] 
    private string _dSM_Five;

    [SerializeField]
    public string _vR_observable_actions;
    
    [ReadOnly, SerializeField] 
    public int value; 

    public HyperActivityMetric(string _dSM_Five, string _vR_observable_actions, int initialValue = 0)
    {
        this._dSM_Five = _dSM_Five;
        this._vR_observable_actions = _vR_observable_actions;
        this.value = initialValue;
    }
    public string DSM_Five => _dSM_Five;
}

public class ADHDParameters : MonoBehaviour
    {
        [SerializeField] 
        public InattentionMetric[] inattentionMetrics = new InattentionMetric[]
        {
            new InattentionMetric(
                "a. 흔히 세부적인 면에 대해 면밀한 주의를 기울이지 못하거나, 학업, 직업, 또는 다른 활동에서 부주의한 실수를 저지른다.",
                "함정 진흙을 누른 횟수 (Count)", 0),
        
            new InattentionMetric(
                "b. 흔히 일 또는 놀이를 할 때 지속적인 주의집중에 어려움이 있다.",
                "오븐이 아닌 다른 곳을 본 시간 (Time)", 0),

            new InattentionMetric(
                "c. 흔히 다른 사람이 직접적으로 말을 할 때 경청하지 않는 것처럼 보인다.",
                "돼지가 설명하면서 말을 걸 때, 알겠어가 아닌 다시 들려줘를 누른 횟수 (Count)", 0),
        
            new InattentionMetric(
                "d. 흔히 지시를 따르지 못하고, 학업, 잡일, 또는 직장에서의 임무를 수행하지 못한다.",
                "0단계에서 힌트를 누른 횟수 (Count)", 0),

            new InattentionMetric(
                "e. 흔히 과업과 활동조직에 어려움이 있다.",
                "0단계에서 잘못된 진흙을 주운 횟수", 0),

            new InattentionMetric(
                "f. 흔히 지속적인 정신적 노력을 요하는 과업에의 참여를 피하고, 싫어하고, 저항한다.",
                "긴 설명이 있는 UI 내에서 일정 시간 (5초) 안에 대충 넘어감 (Selection)", 0),

            new InattentionMetric(
                "g. 흔히 과제나 활동에 필요한 물건들을 분실한다.",
                "총 몇 개의 진흙을 주웠는지 기억해? 오답 횟수", 0),

            new InattentionMetric(
                "h. 흔히 외부자극에 의해 쉽게 산만해진다.",
                "1단계에서 돼지가 말을 걸면서 방해할 때 집중력이 흐트러진 횟수 (Count)", 0),

            new InattentionMetric(
                "i. 흔히 일상 활동에서 잘 잊어버린다.",
                "0단계를 클리어하는데 걸린 시간 (Time)", 0)
        };
        
        [SerializeField] 
        public HyperActivityMetric[] hyperActivityMetric = new HyperActivityMetric[]
        {
            new HyperActivityMetric(
                "a. 흔히 손발을 가만히 두지 못하거나 의자에 앉아서도 몸을 움직거린다.",
                "함정 진흙을 누른 횟수 (Count)", 0),
        
            new HyperActivityMetric(
                "b. 흔히 앉아 있도록 기대되는 교실이나 기타 상황에서 자리를 뜬다.",
                "오븐이 아닌 다른 곳을 본 시간 (Time)", 0),

            new HyperActivityMetric(
                "c. 흔히 부적절한 상황에서 지나치게 뛰어다니거나 기어오른다",
                "돼지가 설명하면서 말을 걸 때, 알겠어가 아닌 다시 들려줘를 누른 횟수 (Count)", 0),
        
            new HyperActivityMetric(
                "d. 흔히 여가활동에 조용히 참여하거나 놀지 못한다. ",
                "0단계에서 힌트를 누른 횟수 (Count)", 0),

            new HyperActivityMetric(
                "e. 흔히 끊임없이 움직이거나 마치 자동차에 쫓기는 것처럼 행동한다.",
                "0단계에서 잘못된 진흙을 주운 횟수", 0),

            new HyperActivityMetric(
                "f. 흔히 지나치게 수다스럽게 말한다.",
                "긴 설명이 있는 UI 내에서 일정 시간 (5초) 안에 대충 넘어감 (Selection)", 0),

            new HyperActivityMetric(
                "g. 흔히 질문이 채 끝나기 전에 성급하게 대답한다.",
                "총 몇 개의 진흙을 주웠는지 기억해? 오답 횟수", 0),

            new HyperActivityMetric(
                "h. 흔히 차례를 기다리지 못한다",
                "1단계에서 돼지가 말을 걸면서 방해할 때 집중력이 흐트러진 횟수 (Count)", 0),

            new HyperActivityMetric(
                "i. 흔히 다른 사람의 활동을 방해하고 간섭한다.",
                "0단계를 클리어하는데 걸린 시간 (Time)", 0)
        };
    }
