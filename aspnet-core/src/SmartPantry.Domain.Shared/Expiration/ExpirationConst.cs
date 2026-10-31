namespace SmartPantry;

public static class ExpirationConst
{
    /// <summary>
    /// Días de anticipación para avisar. El día límite es inclusivo:
    /// un ítem que vence dentro de exactamente este número de días ya genera aviso.
    /// </summary>
    public const int UmbralDiasAviso = 3;
}