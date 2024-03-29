﻿-- Script for Cheat Engine to find offsets
--
-- Make sure you have this plugin enabled: https://forum.cheatengine.org/viewtopic.php?t=587401
-- Copy & paste this into the lua script window and run it

local moduleStart = getAddress("quake_x64_steam.exe");
local moduleEnd = moduleStart + getModuleSize("quake_x64_steam.exe");

function toHex(hex) return string.format("%x", hex) end

function nextInstruction(addr)
    return addr + getInstructionSize(addr);
end

function skipInstructions(start, count)
    local i
    for i = 1, count do start = nextInstruction(start); end
    return start
end

function skipInstructionsUntil(start, callback, backwards)
    local runaway = 0;

    while not callback(getOpCodeAt(start), start) do
        if not backwards then
            start = nextInstruction(start);
        else
            start = getPreviousOpcode(start);
        end

        runaway = runaway + 1;

        if runaway >= 100 then error("runaway. Start: " .. toHex(start)); end
    end

    return start
end


function getOpCodeAt(addr)
    _, code, _, _ = splitDisassembledString(disassemble(addr));
    return string.upper(code);
end


function findPattern(name, pattern, optional)
    local results = AOBScan(pattern, '+X-C-W');

    local count = results.Count

    local filteredResults = {};
    local i;
    for i = 0, results.Count - 1 do
        local a = tonumber(results[i], 16)
        if a >= moduleStart and a <= moduleEnd then
            table.insert(filteredResults, a)
        end
    end

    results.destroy()

    if #filteredResults == 0 then
        if not optional then
            error("No result found");
        end

        return nil;
    end

    if #filteredResults > 1 then
        local str = "";
        for i = 1, #filteredResults do
            str = str .. " " .. toHex(filteredResults[i])
        end

        error("More than 1 result for " .. name .. ": " .. str);
    end

    return filteredResults[1]
end

function runScanForAddress(address)
    local scan = createMemScan(true);
    local foundList = createFoundList(scan);

    scan.firstScan(soExactValue, vtQword, rtTruncated, address, nil,
                   moduleStart, moduleEnd, "*X*C*W", fsmAligned, "4", false,
                   false, false, false);
    scan.waitTillDone();

    foundList.initialize();
    if foundList.Count < 1 then
        foundList.destroy();
        scan.destroy();
        error("Could not find address: " .. address);
    end
    if foundList.Count > 1 then
        foundList.destroy();
        scan.destroy();
        error("More than 1 address");
    end

    local result = foundList.Address[0]
    foundList.destroy()
    scan.destroy()

    return tonumber(result, 16)
end

function offsetTableToJson(tbl)
    table.sort(tbl);

    local str = "{\n";

    local first = true;
    for k, v in pairs(tbl) do
        if not first then
            str = str .. ",\n"
        else
            first = false;
        end

        str = str .. "  \"" .. k .. "\": \"" .. toHex(v) .. "\"";
    end

    str = str .. "\n}";
    return str;
end

function executeOffsetSearch(name,body)
    print("Searching: " .. name);

    local status, val = pcall(body);


    if not status then    
        error("Error while executing offset search for: " .. name .. ". Error: " .. val);
    end

    return val;
end

local offsets = {};

offsets.pr_builtin = executeOffsetSearch("pr_builtin",function()
    local PF_makevectors = findPattern("pr_builtin","48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxx4Cxxxxxxxxxxxx4Cxxxxxxxxxxxx48xxxxxxxxxxxxE9xxxxxxxx")
    local PF_makevectors_addr = runScanForAddress(PF_makevectors);
    return PF_makevectors_addr - 8;
end);

offsets.pr_builtins = executeOffsetSearch("pr_builtins",function()
    return runScanForAddress(offsets.pr_builtin);
end);

offsets.PrintConsole = executeOffsetSearch("PrintConsole",function()
    return findPattern("printConsole","48xxxxxxxx48xxxxxxxx48xxxxxxxxxx41xx41xx41xx41xx48xxxxxxxxxxxx49xxxx4Cxxxx4Cxxxx49xxxxxxxxxxxx")    
end);

offsets.PrintConsoleBuffer = executeOffsetSearch("PrintConsoleBuffer",function()
    local PF_objerror = readPointer(offsets.pr_builtin + (8 * 11));

    local callConsole = skipInstructionsUntil(PF_objerror, function(opcode)
        return string.match(opcode,"^CALL " .. string.upper(toHex(offsets.PrintConsole)));
    end)

    bufferOpcode = skipInstructionsUntil(callConsole, function(opcode)
        return string.match(opcode, "^LEA RCX,%[[%d%a]+%]");
    end, true);

    _, _, address = string.find(getOpCodeAt(bufferOpcode),"^LEA RCX,%[([%d%a]+)%]");
    return tonumber(address, 16);
end);

offsets.SV_SpawnServer = executeOffsetSearch("SV_SpawnServer",function()
    return findPattern("spawnServer","40xxxxxx41xx41xx41xx41xx48xxxxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxxxx48xxxxxxxxxxxxxx4Cxxxx48xxxxxxxxxxxx48xxxx74xx48xxxxFFxxxxxxxxxx");
end);

offsets.CvarGet = executeOffsetSearch("CvarGet",function()
    return findPattern("cvarGet","48xxxxxxxx48xxxxxxxx48xxxxxxxxxx48xxxxxx48xxxxxxxx48xxxx48xxxx75xx48xxxxxx4Cxxxxxxxxxxxx");
end);

offsets.CvarRegister = executeOffsetSearch("CvarRegister",function()
    return findPattern("registerCvar","48xxxxxxxxxx48xxxxxx48xxxxxxxxxxxxxxxx48xxxxxxxx48xxxxxxxx48xxxxxxxx48xxxx48xxxx48xxxx4Cxxxxxx4Cxxxxxx8Bxxxxxx89xxxxF3xxxxxxxxxxF3xxxxxxxxF3xxxxxxxxxxF3xxxxxxxx0FB6xxxxxx  ");
end);

offsets.CvarList = executeOffsetSearch("CvarList",function()
    local cvarList = findPattern("cvarList","48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxx48xxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxx74xx48xxxxxx48xxxxxxxxxxxxFFxxxxxxxxxx");

    _, _, address = string.find(getOpCodeAt(cvarList),"^LEA %a%a%a,%[([%d%a]+)%]");
    return tonumber(address,16);
end);

offsets.CvarLast = executeOffsetSearch("CvarLast",function()
    local lastCvar = findPattern("lastCvar","48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxx48xxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxx74xx48xxxxxx48xxxxxxxxxxxxFFxxxxxxxxxx");
    lastCvar = skipInstructions(lastCvar,1)

    _, _, address = string.find(getOpCodeAt(lastCvar),"^MOV %[([%d%a]+)%]");
    return tonumber(address,16);
end);

--[[
offsets.BuiltinsExtendedMap = (function()
    local registerExtendedBuiltins = findPattern("BuiltinsExtendedMap","48xxxxxxxxxxxxE8xxxxxxxx48xxxxxx48xxxxxx48xxxx75xx48xxxxxxxxxxxx48xxxxxxxxxxxxxx48xxxxxxxxxxxxxxE9xxxxxxxx");

    _, _, address = string.find(getOpCodeAt(registerExtendedBuiltins),"^LEA %a%a%a,%[([%d%a]+)%]");
    return tonumber(address, 16);
end)();
]]--

offsets.pr_functions = executeOffsetSearch("pr_functions",function()
    local pr_functions_get = findPattern("pr_functions","48xxxxxxxxxxxx48xxxxxx44xxxxxxxxxxxx41xxxxxxxxxxC6xxxxxxxxxxxx48xxxxxxE8xxxxxxxx");

    _, _, address = string.find(getOpCodeAt(pr_functions_get),"^MOV %a%a%a,%[([%d%a]+)%]");
    return tonumber(address, 16);
end);

offsets.ED_FindFunction = executeOffsetSearch("ED_FindFunction",function()
    return findPattern("ED_FindFunction","48895C240848896C2410488974241848897C242041564883EC204Cxxxxxxxxxxxx33DB488BE941395924");
end);

offsets.PR_LoadProgs = executeOffsetSearch("PR_LoadProgs",function()
    return findPattern("PR_LoadProgs","40xxxxxx41xx48xxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxxC6xxxxxxxxxxxxB8xxxxxxxxC6xxxxxxxxxxxx48xxxx66xxxxxxxxxxxx");
end);

offsets.pr_builtin_end = executeOffsetSearch("pr_builtin_end",function()
    local count = findPattern("count1","85xx79xxF7xx81xxxxxxxxxx7Cxx48xxxxxxxxxxxxE8xxxxxxxx",true);

    if count == nil then
        count = findPattern("count2","85xx79xxF7xx83xxxx7Cxx48xxxxxxxxxxxxE8xxxxxxxx");
    end

    count = skipInstructionsUntil(count,function(opcode)
        return string.match(opcode,"^CMP");
    end);

    _, _, count = string.find(getOpCodeAt(count),"^CMP %a%a%a?,(%x+)");
    return offsets.pr_builtin + (tonumber(count,16) * 8);
end);

offsets.pr_edict_size = executeOffsetSearch("pr_edict_size",function()
    local base = findPattern("base","45xxxxxx49xxxx49xxxx89xxxxxxxxxx33xx4Cxxxxxxxxxxxx4Cxxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx45xxxx");

    base = skipInstructionsUntil(base,function(opcode)
        return string.match(opcode,"^MOV %[%x+%],EAX")
    end);

    _, _, base = string.find(getOpCodeAt(base),"^MOV %[(%x+)%],EAX");
    return tonumber(base,16);
end);

offsets.pr_edict_size = executeOffsetSearch("pr_edict_size",function()
    local base = findPattern("base","45xxxxxx49xxxx49xxxx89xxxxxxxxxx33xx4Cxxxxxxxxxxxx4Cxxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx45xxxx");

    base = skipInstructions(base,3)

    _, _, base = string.find(getOpCodeAt(base),"^MOV %[(%x+)%]");
    return tonumber(base,16);
end);

offsets.pr_statements = executeOffsetSearch("pr_statements",function()
    local base = findPattern("base","45xxxxxx49xxxx49xxxx89xxxxxxxxxx33xx4Cxxxxxxxxxxxx4Cxxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx45xxxx");

    base = skipInstructions(base,7)

    _, _, base = string.find(getOpCodeAt(base),"^MOV %[(%x+)%]");
    return tonumber(base,16);
end);

offsets.pr_globals = executeOffsetSearch("pr_globals",function()
    local base = findPattern("base","45xxxxxx49xxxx49xxxx89xxxxxxxxxx33xx4Cxxxxxxxxxxxx4Cxxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx45xxxx");

    base = skipInstructions(base,9)

    _, _, base = string.find(getOpCodeAt(base),"^MOV %[(%x+)%]");
    return tonumber(base,16);
end);

offsets.pr_argc = executeOffsetSearch("pr_argc",function()
    local base = findPattern("base","8Dxxxx89xxxxxxxxxx8Bxxxx85xx75xx48xxxxxxxxxxxxE8xxxxxxxx8Bxxxx");

    base = skipInstructions(base,1);

    _, _, base = string.find(getOpCodeAt(base),"^MOV %[(%x+)%]");
    return tonumber(base, 16);
end);

offsets.PR_ExecuteProgram = executeOffsetSearch("PR_ExecuteProgram",function()
    return findPattern("function","48xxxxxxxx48xxxxxxxx48xxxxxxxxxx41xx41xx41xx41xx48xxxxxx0F29xxxxxx48xxxx85xx74xx48xxxxxxxxxxxx3Bxxxx7Cxx48xxxxxxxxxxxx48xxxxxx85xx74xx");
end);

offsets.PR_EnterFunction = executeOffsetSearch("PR_EnterFunction",function()
    return findPattern("function","48xxxxxxxx48xxxxxxxxxx48xxxxxx4Cxxxxxxxxxxxx48xxxxxxxxxxxx8Bxxxxxxxxxx49xxxx48xxxx41xxxx44xxxxxxxxxxxx48xxxx");
end);

offsets.GetPRString = executeOffsetSearch("GetPRString",function()
    return findPattern("function","48xxxxxx48xxxx85xx78xx48xxxxxxxxxxxx33xx48xxxxxxxxxxxx48xxxxxxEBxx83xxxx48xxxxxxxxxxxx48xxxxxx48xxxx74xx");
end);

offsets.ED_FindField = executeOffsetSearch("ED_FindField",function()
    local base = findPattern("base","48xxxxE8xxxxxxxx48xxxx75xx48xxxxxxxxxxxxxx4Cxxxxxxxxxxxx48xxxxxxxx48xxxxxxxx");

    base = skipInstructions(base,1);

    _, _, base = string.find(getOpCodeAt(base),"^CALL (%x+)");
    return tonumber(base,16);
end);

offsets.sv_edicts = executeOffsetSearch("sv_edicts",function()
    local base = findPattern("","48xxxxxxxxxxxx48xxxxxx48xxxx48xxxx75xx83xxxxxxxxxxxx75xx");

    _, _, base = string.find(getOpCodeAt(base),"^MOV %a%a%a,%[(%x+)%]");
    return tonumber(base,16);
end);

offsets.svs = executeOffsetSearch("svs",function()
    local base = findPattern("","C7xxxxxxxxxxxxxxxxxxE8xxxxxxxxBDxxxxxxxx4Cxxxx41xxxxxxxxxx44xxxxxx44xxxx");

    _,_, base = string.find(getOpCodeAt(base),"^MOV %[(%x+)%]");
    return tonumber(base,16);
end);

offsets.gPlayfabClients = executeOffsetSearch("gPlayfabClients",function()
    local base = findPattern("","48xxxxxxxxxxxx48xxxxxx4Cxxxxxxxx4Cxxxxxxxx80xxxxxx0F85xxxxxxxx4Cxxxxxxxx0F1Fxxxxxxxxxxxx48xxxxxxxx49xxxxxx49xxxxxx48xxxxxx48xxxxxxxx");

    _,_, base = string.find(getOpCodeAt(base),"^MOV %a%a%a,%[(%x+)%]");
    return tonumber(base,16);
end);

offsets.CreateEngineString = executeOffsetSearch("CreateEngineString",function()
    return findPattern("","40xx48xxxxxx48xxxxxxxxxxxx48xxxx48xxxx72xx48xxxxxxxxxxxx73xx2Bxx8Bxx48xxxxxxxxxx0FB6xx");
end);

offsets.gTemporaryStringCounter = executeOffsetSearch("gTemporaryStringCounter",function()
    local base = findPattern("","8Bxxxxxxxxxx8Dxxxx23xxxxxxxxxx3Bxxxxxxxxxx89xxxxxxxxxx72xx41xxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxxE8xxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxx48xxxxxxxxxxxx");

    _,_,base = string.find(getOpCodeAt(base),"^MOV EBX,%[(%x+)%]");
    return tonumber(base,16);
end);

offsets.gTemporaryStringMax = executeOffsetSearch("gTemporaryStringMax",function()
    local base = findPattern("","8Bxxxxxxxxxx8Dxxxx23xxxxxxxxxx3Bxxxxxxxxxx89xxxxxxxxxx72xx41xxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxxE8xxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxx48xxxxxxxxxxxx");

    base = skipInstructions(base,2);

    _,_,base = string.find(getOpCodeAt(base),"^AND EBX,%[(%x+)%]");
    return tonumber(base,16);
end);

offsets.gTemporaryStringBase = executeOffsetSearch("gTemporaryStringBase",function()
    local base = findPattern("","8Bxxxxxxxxxx8Dxxxx23xxxxxxxxxx3Bxxxxxxxxxx89xxxxxxxxxx72xx41xxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxxE8xxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxx48xxxxxxxxxxxx");

    base = skipInstructions(base,13);

    _,_,base = string.find(getOpCodeAt(base),"^ADD RBX,%[(%x+)%]");
    return tonumber(base,16);
end);


offsets.builtins_idCheck = executeOffsetSearch("builtins_idCheck",function()
    return findPattern("","F7xx81xxxxxxxxxx7Cxx48xxxxxxxxxxxxE8xxxxxxxx48xxxx48xxxxxxxxxxxxFFxxxxxxxxxxxxE9xxxxxxxxE8xxxxxxxxE9xxxxxxxx83xxxxxxxxxxxx48xxxxxxxxxxxx");
end);

offsets.builtins_call = executeOffsetSearch("builtins_call",function()
    local addr;

    local base = offsets.builtins_idCheck;

    base = skipInstructions(base,7);

    return base;
end);

print(" ");
print(" ");
print(offsetTableToJson(offsets));
