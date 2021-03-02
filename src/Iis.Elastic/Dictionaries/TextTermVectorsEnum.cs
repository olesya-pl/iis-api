namespace Iis.Elastic.Dictionaries
{
    public enum TextTermVectorsEnum
    {
        //no term vectors are stored
        No = 0,
        //just the terms in the field are stored
        Yes,
        //terms and positions are stored
        WithPositions,
        //terms and character offsets are stored
        WithOffsets,
        //terms, positions, and character offsets are stored
        WithPositionsOffsets
    }
}