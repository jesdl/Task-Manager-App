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
    public class DevTaskHelper
    {
        public virtual Project Project { get; set; }
        public virtual DevTask DevTask { get; set; }

        static ApplicationDbContext db = new ApplicationDbContext();

        static UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>
            (new UserStore<ApplicationUser>(db));

        static RoleManager<IdentityRole> projectManager = new RoleManager<IdentityRole>
            (new RoleStore<IdentityRole>(db));

        public List<string> GetAllTasks()
        {
            var result = db.DevTasks.Select(t => t.Name).ToList();

            return result;
        }

        public static ICollection<int> TasksForUser(string userId)
        {
            var result = db.DevTasks.Where(up => up.ApplicationUsers.Select(d => d.Id).Contains(userId)).Select(t => t.Id);

            return result.ToList();
        }
        public static ICollection<int> TasksForProject(int projectId)
        {
            var result = db.DevTasks.Where(up => up.Project.Id == projectId).Select(t => t.Id);

            return result.ToList();
        }
        public static DevTask CreateDevTask(string name, string description, DateTime deadline, Project project)
        {
            DevTask newTask = new DevTask
            {
                Name = name,
                Description = description,
                StartDate = DateTime.Now,
                //Deadline = deadline,
                ProjectId = project.Id,
                PercentCompleted = 0,
                IsComplete = false,
            };
            return newTask;
        }
        public static void AssignDevTask(ApplicationUser user, DevTask task)
        {
            if(UserManager.checkUserRole(user.Id, "Developer"))
            {
                user.DevTasks.Add(task);
                task.ApplicationUsers.Add(user);
            }
        }
        public static void AssignDevsToTask(List<ApplicationUser> devs, DevTask task)
        {
            foreach(ApplicationUser dev in devs)
            {
                if (!dev.DevTasks.Contains(task) && !task.ApplicationUsers.Contains(dev))
                {
                    AssignDevTask(dev, task);
                }
            }
        } 
        public static void UpdateDevTask(DevTask task)
        {
            
        }
        public static void  DeleteDevTask(DevTask task)
        {
            db.DevTasks.Remove(task);
        }

        public static void AddComment(string comment, DevTask task)
        {
            task.Comments.Add(comment);
        }
        //AddNote
        public static void UpdateCompletionPercent(double newValue, DevTask task)
        {
            if (newValue <= 100)
            {
                task.PercentCompleted = newValue;
                if (newValue == 100)
                {
                    task.IsComplete = true;
                }
                else
                {
                    task.IsComplete = false;
                }
            }
            else
            {
                //error
            }
        }
        public static void SendNotification(string title, string description, ApplicationUser user, DevTask task)
        {
            Notification notification = new Notification
            {
                Title = title,
                Description = description,
                ApplicationUserId = user.Id,
                isOpened = false,
                ProjectId = task.ProjectId,
                DevTaskId = task.Id,
            };
            user.Notifications.Add(notification);
            
        }
        //SendDeadlineAlert(Project) 
        public static void SendBugNotification(DevTask task, ApplicationUser recipient,string title, string description)
        {
            int projectId = task.Project.Id;

        }

    }
}