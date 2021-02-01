using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Commander.Models;
using Commander.Exceptions;

namespace Commander.Data
{
    public class SqlCommanderRepo : ICommanderRepo
    {
        private readonly CommanderContext _context;

        public SqlCommanderRepo(CommanderContext context)
        {
            _context = context;
        }

        public void CreateCommand(Command cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException(nameof(cmd));
            }

            if (_context.Commands.Any(p => p.Line == cmd.Line))
                throw new AppException("Command already exists");

            if (_context.Commands.Any(p => p.HowTo == cmd.HowTo))
                throw new AppException("There is already a command with that description");

            _context.Commands.Add(cmd);
        }

        public IEnumerable<Command> GetAllCommands()
        {
            return _context.Commands.ToList();
        }

        public Command GetCommandById(int id)
        {
            return _context.Commands.FirstOrDefault(p => p.Id == id);
        }

        public IEnumerable<Command> GetCommandsByPlatform(string platform)
        {
            return _context.Commands.Where(p => p.Platform == platform).ToList();
        }

        public bool SaveChanges()
        {
            return (_context.SaveChanges() >= 0);

        }

        public void UpdateCommand(Command cmd)
        {
            //nothing
        }

        public void DeleteCommand(Command cmd)
        {
            if(cmd==null){
                throw new ArgumentNullException(nameof(cmd));
            }

            _context.Commands.Remove(cmd);
        }
    }
}