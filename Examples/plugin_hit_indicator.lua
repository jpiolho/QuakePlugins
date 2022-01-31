local hitSound = "fish/bite.wav"

function QC_T_Damage()
    local target = QC.GetEdict(QC.Value.Parameter0)
    local inflictor = QC.GetEdict(QC.Value.Parameter1)
    local attacker = QC.GetEdict(QC.Value.Parameter2)
    local damage = QC.GetFloat(QC.Value.Parameter3)
    
    -- We only care about players that are attacking
    if attacker.Classname == "player" then
    
        -- Check if target is a player or a monster
        if target.Classname == "player" or string.sub(target.Classname,1,8) == "monster_" then
            Builtins.Stuffcmd(attacker,"bf\n")
            Builtins.LocalSound(attacker,hitSound);
        end
    end
end



function QC_Worldspawn()
    Builtins.PrecacheSound(hitSound)
end

Hooks.RegisterQC("T_Damage",QC_T_Damage)
Hooks.RegisterQCPost("worldspawn",QC_Worldspawn)