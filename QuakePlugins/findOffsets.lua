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


function findPattern(name, pattern)
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

    if #filteredResults == 0 then error("No result found") end

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


local offsets = {};

offsets.Builtins = (function()
    local PF_makevectors = findPattern("builtins","48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxx4Cxxxxxxxxxxxx4Cxxxxxxxxxxxx48xxxxxxxxxxxxE9xxxxxxxx")
    local PF_makevectors_addr = runScanForAddress(PF_makevectors);
    return PF_makevectors_addr - 8;
end)();

offsets.BuiltinsPointer = (function()
    return runScanForAddress(offsets.Builtins);
end)();

offsets.PrintConsole = (function()
    return findPattern("printConsole","48xxxxxxxx48xxxxxxxx48xxxxxxxxxx41xx41xx41xx41xx48xxxxxxxxxxxx49xxxx4Cxxxx4Cxxxx49xxxxxxxxxxxx")    
end)();

offsets.PrintConsoleBuffer = (function()
    local PF_objerror = readPointer(offsets.Builtins + (8 * 11));

    local callConsole = skipInstructionsUntil(PF_objerror, function(opcode)
        return string.match(opcode,"^CALL " .. string.upper(toHex(offsets.PrintConsole)));
    end)

    bufferOpcode = skipInstructionsUntil(callConsole, function(opcode)
        return string.match(opcode, "^LEA RCX,%[[%d%a]+%]");
    end, true);

    _, _, address = string.find(getOpCodeAt(bufferOpcode),"^LEA RCX,%[([%d%a]+)%]");
    return tonumber(address, 16);
end)();

offsets.SV_SpawnServer = (function()
    return findPattern("spawnServer","40xxxxxx41xx41xx41xx41xx48xxxxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxxxx48xxxxxxxxxxxxxx4Cxxxx48xxxxxxxxxxxx48xxxx74xx48xxxxFFxxxxxxxxxx");
end)();

offsets.CvarGet = (function()
    return findPattern("cvarGet","48xxxxxxxx48xxxxxxxx48xxxxxxxxxx48xxxxxx48xxxxxxxx48xxxx48xxxx75xx48xxxxxx4Cxxxxxxxxxxxx");
end)();

offsets.CvarRegister = (function()
    return findPattern("registerCvar","48xxxxxxxxxx48xxxxxx48xxxxxxxxxxxxxxxx48xxxxxxxx48xxxxxxxx48xxxxxxxx48xxxx48xxxx48xxxx4Cxxxxxx4Cxxxxxx8Bxxxxxx89xxxxF3xxxxxxxxxxF3xxxxxxxxF3xxxxxxxxxxF3xxxxxxxx0FB6xxxxxx  ");
end)();

offsets.CvarList = (function()
    local cvarList = findPattern("cvarList","48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxx48xxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxx74xx48xxxxxx48xxxxxxxxxxxxFFxxxxxxxxxx");

    _, _, address = string.find(getOpCodeAt(cvarList),"^LEA %a%a%a,%[([%d%a]+)%]");
    return tonumber(address,16);
end)();

offsets.CvarLast = (function()
    local lastCvar = findPattern("lastCvar","48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxx48xxxxxx48xxxxxxxxxxxx48xxxxxxxxxxxx48xxxx74xx48xxxxxx48xxxxxxxxxxxxFFxxxxxxxxxx");
    lastCvar = skipInstructions(lastCvar,1)

    _, _, address = string.find(getOpCodeAt(lastCvar),"^MOV %[([%d%a]+)%]");
    return tonumber(address,16);
end)();

print(offsetTableToJson(offsets));
