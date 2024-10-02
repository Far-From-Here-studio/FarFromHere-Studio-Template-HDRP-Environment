
bool StartEvent(inout float _time, inout float _result)
{
    _result = 0;
    _time = 0;
    
    return false;
}

void StartTimer_float(in bool startEvent, in float TimeLenght ,out float result)
{    
    float time;
    
    if (startEvent) startEvent = StartEvent(time, result);
    
    if (startEvent == false)
    {
        time = time + (unity_DeltaTime.x * 4);
        result = time;
    }
    
    

}
