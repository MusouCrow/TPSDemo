local _SKYNET = require("src.skynet")
local _SOCKET = require("src.socket")
local _ID = require("src.id")
local _TABLE = require("src.table")

local _gate
local _fds = {}
local _inputsMap = {}
local _playSender = {}
local _FUNC = {}
local _CMD = {}
local _playFrame = 1
local _timer = _SKYNET.now()
local _readyPlay = false

function _FUNC.NewActor(fd)
    return {addr = _SOCKET.ToAddress(fd), x = math.random(-300, 300) * 0.01, y = math.random(-300, 300) * 0.01}
end

function _FUNC.Send(id, obj)
    _SKYNET.Send(_gate, "Send", _fds, id, obj)
end

function _FUNC.Play()
    _playSender.addrs = {}
    _playSender.inputs = {}
    _playSender.playFrame = _playFrame

    for k, v in pairs(_inputsMap) do
        if (#v > 0) then
            table.insert(_playSender.addrs, _SOCKET.ToAddress(k))
            table.insert(_playSender.inputs, v)
        end
    end

    if (#_playSender.addrs == 0) then
        _playSender.addrs = nil
    end

    if (#_playSender.inputs == 0) then
        _playSender.inputs = nil
    end

    _FUNC.Send(_ID.input, _playSender)
    _TABLE.Clear(_inputsMap)
    _readyPlay = false
    _timer = _SKYNET.now()
    _playFrame = _playFrame + 1
end

function _CMD.Exit()
    _SKYNET.exit()
end

function _CMD.Start(leftFd, rightFd)
    _fds = {leftFd, rightFd}
    _FUNC.Send(_ID.start, {seed = os.time(), left = _FUNC.NewActor(leftFd), right = _FUNC.NewActor(rightFd)})
end

function _CMD.ReceiveInput(fd, obj)
    _inputsMap[fd] = obj.inputs

    if (obj.playFrame == _playFrame) then
        if (not _readyPlay) then
            _readyPlay = true
        else
            _FUNC.Play()
        end
    end
end

local function _Start()
    _gate = _SKYNET.queryservice("gate")
    _SKYNET.DispatchCommand(_CMD)
end

_SKYNET.start(_Start)