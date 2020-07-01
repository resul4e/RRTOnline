using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshPro))]
public class ValueChanger : MonoBehaviour
{
	public string Postfix= "%";
	public bool PostSpace = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ChangeValue(float _val)
    {
	    GetComponent<TextMeshProUGUI>().text = _val.ToString() + (PostSpace ? " " : "") + Postfix;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
