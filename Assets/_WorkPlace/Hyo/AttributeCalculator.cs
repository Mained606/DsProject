public static class AttributeCalculator
{
    public static float GetMultiplier(ElementalAttribute attacker, ElementalAttribute defender)
    {
        // 무속성이거나 둘 중 하나가 무속성인 경우 기본 배율 1.0 적용
        if (attacker == ElementalAttribute.None || defender == ElementalAttribute.None)
            return 1.0f;

        switch (attacker)
        {
            case ElementalAttribute.Fire:
                if (defender == ElementalAttribute.Earth) return 2.0f;
                else if (defender == ElementalAttribute.Water) return 0.5f;
                break;
            case ElementalAttribute.Water:
                if (defender == ElementalAttribute.Fire) return 2.0f;
                else if (defender == ElementalAttribute.Electric) return 0.5f;
                break;
            case ElementalAttribute.Electric:
                if (defender == ElementalAttribute.Water) return 2.0f;
                else if (defender == ElementalAttribute.Fire) return 0.5f;
                break;
            case ElementalAttribute.Earth:
                if (defender == ElementalAttribute.Electric) return 2.0f;
                else if (defender == ElementalAttribute.Water) return 0.5f;
                break;
        }
        // 나머지 경우에는 기본 배율 1.0 적용
        return 1.0f;
    }
}
