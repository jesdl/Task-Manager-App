﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TaskManagerProject.Models
{
    [Authorize(Roles = "ProjectManager")]
    public class ProjectHelper
    {
        static ApplicationDbContext db = new ApplicationDbContext();

        static UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>
            (new UserStore<ApplicationUser>(db));

        static RoleManager<IdentityRole> projectManager = new RoleManager<IdentityRole>
            (new RoleStore<IdentityRole>(db));

        
        public ICollection<Project> GetAllProjects()
        {
            var result = db.Projects.ToList();

            return result;
        }

        public ICollection<string> ProjectsForUser(int userId)
        {
            var result = db.Projects.Where(up => up.Id == userId).Select(p => p.Name);
            return result.ToList();
        }

        public void AddProject(Project projectName)
        {
            if (!db.Projects.Contains(projectName))
            {
                var newProject = db.Projects.Add(projectName);
            }
            else
            {
                throw new Exception("Project Already Exists");
            }
        }

        public void DeleteProject(Project projectId)
        {
            if (!db.Projects.Contains(projectId))
            {
                db.Projects.Remove(projectId);
            }
            else
            {
                throw new Exception("Project Does Not Exist");
            }
        }
    }
}