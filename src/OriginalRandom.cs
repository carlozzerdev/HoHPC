namespace HoH26;

/// <summary>Native translation of CPC Z80 routine 4497–44C5.</summary>
public sealed class OriginalRandom
{
    public ushort StateLow { get; private set; }
    public ushort StateHigh { get; private set; }

    public OriginalRandom(ushort stateLow = 0x6f4a, ushort stateHigh = 0x216e)
    {
        StateLow = stateLow;
        StateHigh = stateHigh;
    }

    public ushort NextUInt16()
    {
        // ADD HL,HL; ADC HL,HL. Preserve carry into the following RL B.
        int doubled = StateHigh << 1;
        int rotated = ((doubled & 0xffff) << 1) | (doubled >> 16);
        int carry = rotated >> 16;
        int c = (rotated >> 8) & 255;
        int highLow = StateLow >> 8;
        int b = ((highLow << 1) | carry) & 255;
        carry = highLow >> 7;
        int e = ((highLow << 1) | carry) & 255;
        carry = highLow >> 7;
        int d = (((StateHigh & 255) << 1) | carry) & 255;

        int lowSum = StateLow + (b << 8) + c;
        int highSum = StateHigh + (d << 8) + e + (lowSum >> 16);
        StateLow = (ushort)lowSum;
        StateHigh = (ushort)(highSum & 0x7fff);

        // RES 7,H does not alter flags: JP M tests ADC's pre-mask sign.
        // The INC byte loop propagates wrap through both little-endian words.
        if ((highSum & 0x8000) == 0)
        {
            StateLow = unchecked((ushort)(StateLow + 1));
            if (StateLow == 0) StateHigh = unchecked((ushort)(StateHigh + 1));
        }
        return StateLow;
    }
}
