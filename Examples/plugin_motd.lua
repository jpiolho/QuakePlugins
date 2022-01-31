-- Motd plugin
-- By JPiolho

local cvar_motd_title = Cvars.Register("motd_title","<MOTD>","The title of the motd (the name of the person who sents it)");
local cvar_motd = Cvars.Register("motd","Welcome to my server","What motd message should be sent");


local MSG_ONE = 1
local SVC_BOTCHAT = 38
local SVC_UPDATENAME = 13



local pendingMessages = {}

function FindHost()
    local e = QC.GetWorld()
    
    repeat
        e = Builtins.NextEnt(e)
        
        if e.Classname ~= nil and e.Classname == "player" then
            return e
        end
        
    until (e.Classname ~= nil and e.Classname == "worldspawn")

    return nil
end

function FindPlayerByName(netname)
    local e = QC.GetWorld()
    
    repeat
        e = Builtins.NextEnt(e)
        
        if e.Classname ~= nil and e.Classname == "player" and e:GetFieldString("netname") == netname then
            return e
        end
        
    until (e.Classname ~= nil and e.Classname == "worldspawn")

    return nil
end

function SendMotd(player)    
    QC.SetMsgEntity(player)
    
    -- Change host name to trick the client
    Builtins.WriteByte(MSG_ONE,SVC_UPDATENAME)
    Builtins.WriteByte(MSG_ONE,0) -- Player 0 (Host)
    Builtins.WriteString(MSG_ONE,cvar_motd_title:GetString())
    
    -- Send the message
    Builtins.WriteByte(MSG_ONE,SVC_BOTCHAT)
    Builtins.WriteByte(MSG_ONE,0) -- Who's talking? (The host!)
    Builtins.WriteShort(MSG_ONE,1) -- How many strings
    Builtins.WriteString(MSG_ONE,cvar_motd:GetString())
    
    -- Restore host name
    local host = FindHost()
    
    Builtins.WriteByte(MSG_ONE,SVC_UPDATENAME)
    Builtins.WriteByte(MSG_ONE,0)
    Builtins.WriteString(MSG_ONE,host:GetFieldString("netname"))
end


function QC_ClientConnect()
    pendingMessages[QC.GetSelf():GetFieldString("netname")] = QC.GetTime() + 1.0
end


function QC_StartFrame()
    local currentTime = QC.GetTime()
    
    for k,v in pairs(pendingMessages) do
        if currentTime >= v then
            SendMotd(FindPlayerByName(k))
            
            pendingMessages[k] = nil
        end
        
    end
end

Hooks.RegisterQC("ClientConnect",QC_ClientConnect)
Hooks.RegisterQC("StartFrame",QC_StartFrame)