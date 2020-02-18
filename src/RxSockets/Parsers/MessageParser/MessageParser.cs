﻿using RxSockets.Abstractions;
using RxSockets.Models;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace RxSockets.Parsers.MessageParser
{
  public class MessageParser : IParser
  {
    private MessageParserSettings _settings;
    public SequencePosition? start;
    public SequencePosition? end;
    private ISubject<ReadOnlySequence<byte>> _whenMessageParsed;

    public MessageParser()
    {
      _whenMessageParsed =  new Subject<ReadOnlySequence<byte>>();
      _settings = new MessageParserSettings()
      {
        StartDelimiter = new byte[1]{ (byte) 2 },
        EndDelimiter = new byte[1]{ (byte) 3 }
      };
    }

    public MessageParser(MessageParserSettings settings)
      : this()
    {
      _settings = settings;
    }

    public IObservable<ReadOnlySequence<byte>> WhenMessageParsed
    {
      get
      {
        return _whenMessageParsed.AsObservable();
      }
    }

        public bool CanParse(ReadOnlySequence<byte> buffer)
        {
            bool startFound = false;
            bool endFound = false;
            if (_settings.StartDelimiter != null && _settings.StartDelimiter.Length > 0)
            {
                for (int i = 0; i < _settings.StartDelimiter.Length; i++)
                {
                    start = buffer.PositionOf(_settings.StartDelimiter[i]);
                    if (!start.HasValue)
                    {
                        start = null;
                        break;
                    }
                }
                if (start.HasValue)
                {
                    buffer = buffer.Slice(start.Value);
                    startFound = true;
                }
            }
            else
            { startFound = true; }
            if (_settings.EndDelimiter != null && _settings.EndDelimiter.Length > 0)
            {
                end = buffer.PositionOf(_settings.EndDelimiter[0]);
                endFound = end.HasValue;
                if (endFound)
                {
                    for (int i = 1; i < this._settings.EndDelimiter.Length; i++)
                    {

                        var auxEnd = buffer.PositionOf(_settings.EndDelimiter[i]);

                        if (!auxEnd.HasValue)
                        {
                            auxEnd = null;
                            endFound = false;
                            break;
                        }
                    }
                }
            }
            return startFound & endFound;
        }

    public Task<ParseResult> ProcessAsync(ReadOnlySequence<byte> buffer)
    {
      ReadOnlySequence<byte> message = buffer.Slice(buffer.GetPosition(_settings.StartDelimiter.Length, start.Value), end.Value);
      return Task.FromResult(new ParseResult()
      {
        CleanMessage = message,
        EndPosition = end.Value
      });
    }

    public Task<ReadOnlySequence<byte>> PrepareMessageToBeSent(
      ReadOnlySequence<byte> bytes)
    {
      List<byte> byteList = new List<byte>();
      byteList.AddRange( _settings.StartDelimiter);
      byteList.AddRange( bytes.ToArray());
      byteList.AddRange(_settings.EndDelimiter);
      return Task.FromResult(new ReadOnlySequence<byte>(byteList.ToArray()));
    }
  }
}
