-- Motd plugin
-- By JPiolho

local options = {
    title = "<MOTD>",
    message = "Welcome to my server"
}

local MSG_ONE = 1
local SVC_BOTCHAT = 38
local SVC_UPDATENAME = 13



local pendingMessages = {}

function FindHost()
    local world = QC.GetWorld()
    local e = QC.GetWorld()
    
    repeat
        e = Builtins.NextEnt(e)
        
        if e.Classname ~= nil and e.Classname == "player" then
            return e
        end
        
    until (e.Classname ~= nil and e.Classname == "worldspawn") or runaway > 10000

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
    Builtins.WriteString(MSG_ONE,options.title)
    
    -- Send the message
    Builtins.WriteByte(MSG_ONE,SVC_BOTCHAT)
    Builtins.WriteByte(MSG_ONE,0) -- Who's talking? (The host!)
    Builtins.WriteShort(MSG_ONE,1) -- How many strings
    Builtins.WriteString(MSG_ONE,options.message)
    
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