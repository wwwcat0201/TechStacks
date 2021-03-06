<template>
  <v-layout column>
    <v-flex v-if="post" class="post-features">

        <v-flex class="post-features">

            <h2 v-if="post.comments.length > 1" class="comments-title">All {{post.comments.length || ''}} comments</h2>
            <div v-else-if="!isAuthenticated" class="comments-signin">Please <a :href="routes.authGitHub">Sign In</a> to comment</div>
            <div v-else class="comments-title">Leave a comment</div>

            <CommentEdit ref="txtComments" v-if="canCommentPost(post)" :post="post" @done="commentDone"></CommentEdit>    
            
        </v-flex>
        <v-flex class="post-comments">
            <PostComment v-for="comment in parentComments" :key="comment.id" :post="post" :comment="comment" :hide="['pin']"></PostComment>
        </v-flex>
    </v-flex>
    <v-flex v-else-if="!loading && !notFound" class="post-features">
      <div v-if="!isAuthenticated" class="comments-signin">Please <a :href="routes.authGitHub">Sign In</a> to comment</div>
      <div v-else>
        <h2 class="comments-title">Leave a comment</h2>

        <v-form v-model="valid" ref="form" lazy-validation>
            <v-card style="margin-top:.5em">
                <v-card-text style="padding-top:0">
                    <v-alert outline color="error" icon="warning" :value="errorSummary">{{ errorSummary }}</v-alert>                  

                    <Editor ref="editor"
                        style="padding-top:10px"
                        label="Comment"
                        v-model="content"
                        :rows="6"
                        :counter="contentCounter"
                        :rules="contentRules"
                        :error-messages="errorResponse('content')"
                        @save="submit"
                        />

                </v-card-text>
                <v-card-actions>
                    <v-layout>
                        <v-btn flat @click="submit">Submit</v-btn>
                    </v-layout>
                </v-card-actions>
            </v-card>
        </v-form>
      </div>
    </v-flex>
  </v-layout>
</template>

<script>
import ReportDialog from "~/components/ReportDialog.vue";
import PostComment from "~/components/PostComment.vue";
import CommentEdit from "~/components/CommentEdit.vue";
import PostAlerts from "~/components/PostAlerts.vue";
import Editor from "@servicestack/editor";

import { mapGetters } from "vuex";
import { routes } from "~/shared/routes";
import { toObject, errorResponse, errorResponseExcept } from "@servicestack/client";
import { createOrganizationForTechnology, createOrganizationForTechStack, createPostComment } from "~/shared/gateway";
import { canCommentPost, sortComments } from "~/shared/post";
import { contentCounter, contentRules } from "~/shared/utils";

const comment = {
  content: null,
};

export default {
  components: { PostComment, ReportDialog, CommentEdit, PostAlerts, Editor },
  props: ["technology", "techstack"],

  computed: {
    errorSummary(){
      return errorResponseExcept.call(this, Object.keys(comment));
    },
    tech(){
      return this.technology || this.techstack;
    },
    post(){
      return this.getPost(this.postId);
    },
    parentComments() {
      return sortComments(this.post.comments.filter(x => x.replyId == null));
    },
    ...mapGetters(["loading", "isAuthenticated", "getPost", "organization"])
  },

  methods: {
    async commentDone(changed){
      await this.loadPost();
    },

    async loadOrganization(){
      if (this.tech && this.tech.organizationId){
        this.postId = this.tech.commentsPostId;
        this.$store.dispatch('loadUserPostCommentVotes', this.postId);
        await this.$store.dispatch('loadOrganizationByIdIfNotExists', this.tech.organizationId);
        await this.loadPost();
      }
    },
    async loadPost() {
      try {
        await this.$store.dispatch("loadPost", this.postId);
      } catch(e) {
        this.notFound = true;
      }
    },

    async reset(){
      this.content = null;
      await this.loadOrganization();
    },
   
    async submit() {
      if (this.$refs.form.validate()) {
        try {
          this.$store.commit('loading', true);
            
          if (this.organizationId == null){
            const orgResponse = this.technology
              ? await createOrganizationForTechnology(this.technology.id)
              : await createOrganizationForTechStack(this.techstack.id);
            
            this.organizationId = orgResponse.organizationId;
            this.postId = orgResponse.commentsPostId;
            this.$emit('organizationCreated', orgResponse);
          }
          
          const response = await createPostComment(this.postId, this.content);

          await this.reset(true);

        } catch(e) {
          this.valid = false;
          this.responseStatus = e.responseStatus || e;
        } finally {
          this.$store.commit('loading', false);
        }
      }
    },

    canCommentPost,
    errorResponse,
  },

  watch: {
    async tech(v){
      await this.loadOrganization();
    }
  },

  async mounted(){
    await this.loadOrganization();
  },

  data: () => ({
    routes,
    ...comment,
    postId: null,
    valid: true,
    contentCounter, contentRules,
    notFound: false,
    responseStatus: null,
  }),
  
}
</script>
